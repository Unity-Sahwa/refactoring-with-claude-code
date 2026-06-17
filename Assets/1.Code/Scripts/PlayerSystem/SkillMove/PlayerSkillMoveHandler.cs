using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Refactoring
{
    // 책임: 플레이어 상태 이벤트를 구독해 정해진 시간 동안 현재 캐릭터를 직접 이동시킨다.
    // 확장성: 없다. 플레이어 스킬 상태에서만 작동한다.
    // 흐름: 플레이어 상태 이벤트 호출 -> HandleSkillMove(...) 호출 -> 오브젝트 활성화 -> FixedUpdate에서 수명 누적·만료 처리 + 물리이동. 수명이 다 끝나면 비활성화
    // 핵심: BaseCharacter 캐릭터오브젝트 존재 / 외부에서 HandleSkillMove(IStartData data) 호출 / FixedUpdate에서 물리이동

    public class PlayerSkillMoveHandler : MonoBehaviour
    {
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber; 
        [Inject(true)] private ICharacterSwapNotifier _swapNotifier; 
        [Inject] private List<BaseCharacter<PlayerCharacterType>> _characters;
         
        private readonly Dictionary<PlayerCharacterType, Rigidbody> _characterRB = new();
        private PlayerCharacterType _currentCharacter;
        private readonly List<ActiveMove> _actives = new();

        private class ActiveMove
        {
            public ISkillMove data;
            public Vector3 localVelocity; // Direction.normalized * Speed 미리 계산 (매 프레임 재정규화 방지)
            public float elapsed;         // 경과 시간 누적 (Duration 도달 시 만료)
        }

        //왜: 시간에 구애 받지 않는다면 가장 빠른 Awake() 단계에서 초기화 처리하는게 Start()에 여유있음.
        private void Awake()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Subscribe(StateEventCategory.SkillMove, HandleSkillMove);
                _eventSubscriber.SubscribeReset(HandleReset);
            }
                
            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped += OnCharacterSwapped;
            }

            // 필수: 캐릭터(RB) 캐싱
            foreach (var character in _characters)
            {
                _characterRB[character.Type] = character.GetCharacterComponent<Rigidbody>();
            }
            if (_characterRB.Count == 0)
            {
                Debug.LogWarning("[PlayerSkillMoveHandler] BaseCharacter 미주입으로 기능 실행 X");
            }

            //이동 호출 전까지 비활성화
            enabled = false;
        }

        //왜: ICharacterSwapNotifier.CurrentCharacter는 Awake() 단계에서 있을지 미지수기 때문에 Start()에서 할당받음
        private void Start()
        {
            if (_swapNotifier != null) _currentCharacter = _swapNotifier.CurrentCharacter;
            else
            {
                foreach (var key in _characterRB.Keys)
                {
                    _currentCharacter = key; 
                    break;
                }
            }
        }

        //호출조건: 구독중인 플레이어 상태이벤트(또는 외부)에 의해 호출
        //왜 : 활성 이동 목록에 추가하고 시스템(FixedUpdate)을 켠다
        public void HandleSkillMove(IStartData data)
        {
            if(data is not ISkillMove skillMoveData) 
            {
                Debug.LogError($"[PlayerSkillMoveHandler] ISkillMove가 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            var active = new ActiveMove {data = skillMoveData, 
                                        localVelocity = skillMoveData.Direction.normalized * skillMoveData.Speed };
            _actives.Add(active);
            enabled = true;
        }

        //호출조건: HandleSkillMove로 `enabled = true` 되면 매 물리프레임 작동
        //왜: 수명을 누적·만료 처리하고, 남은 이동이 있으면 물리 이동시킨다
        // 대원_TODO: 주변 감지로 이동량 변화 확장 예정.
        private void FixedUpdate()
        {
            if (_actives.Count == 0) 
            {
                return;
            }

            // 역순으로 순회해야 다음 요소가 삭제되어도 순서에 영향이 없음
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                _actives[i].elapsed += Time.fixedDeltaTime;
                if (_actives[i].elapsed >= _actives[i].data.Duration) _actives.RemoveAt(i);
            }

            if (_actives.Count == 0) 
            {
                enabled = false;
                return;
            }

            if (!_characterRB.TryGetValue(_currentCharacter, out var rb) || rb == null) 
            {
                return;
            }

            Vector3 worldVelocity = Vector3.zero;
            //속도 합산해서 적용
            //대원_TODO: 물리이동이 의도한 모양대로 나올까?
            foreach (var active in _actives)
            {
                //속도가 플레이어 방향으로 회전된 다음 더해줌.
                worldVelocity += rb.rotation * active.localVelocity;
            }

            Vector3 intended = worldVelocity * Time.fixedDeltaTime;
            if (intended == Vector3.zero) 
            {
                return;
            }

            rb.MovePosition(rb.position + intended);
        }

        // 호출조건: 플레이어 상태 바뀔 때 호출되는 Reset 이벤트에 의해 자동 호출
        // 왜: 상태가 전환되었는데도 이동이 계속되는 것을 방지 + 시스템 종료
        private void HandleReset()
        {
            _actives.Clear();
            enabled = false;
        }

        //호출조건: ICharacterSwapNotifier.OnCharacterSwapped 호출시 자동 호출
        //왜: 2개의 캐릭터가 전환되는 시스템이라, RB도 그에 맞게 변해야 함
        private void OnCharacterSwapped(PlayerCharacterType type)
        {
            _currentCharacter = type;
        }

        //호출조건: 오브젝트 파괴시 호출
        //왜: 이벤트 구독해제 등 변수 차단
        private void OnDestroy()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe(StateEventCategory.SkillMove, HandleSkillMove);
                _eventSubscriber.UnsubscribeReset(HandleReset);
            }

            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped -= OnCharacterSwapped;
            }

            _actives.Clear();
        }
    }
}
