using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 캐릭터에 붙어서 한 번에 하나의 상태만 진행되도록 관리. 규칙에 따른 상태 직접 전환
    public class PlayerStateMachine : MonoBehaviour
    {
        // 캐릭터별 상태 데이터 묶음(Inspector 드래그앤드롭). 클립명 + 이벤트 데이터.
        [Serializable] private class StateEntry
        {
            public PlayerStateType key;
            public string animationName;   // 캐릭터별 클립명(HIdle / AIdle 등)
            public BaseStateData data;     // 진행률 이벤트 데이터(SO)
        }
        [SerializeField] private List<StateEntry> _stateEntries = new List<StateEntry>(); // 데이터가 없어도 상태는 생성됨
        [Inject] private IPlayerStateEventRaiser _raiser;
        [Inject] private IStateTriggerSubscriber _triggerSubscriber;   // 외부(Movement 등)가 쏜 전환 트리거를 받는 채널

        private readonly Dictionary<PlayerStateType, StateRunner> _states = new Dictionary<PlayerStateType, StateRunner>();
        private PlayerCharacter _character;   // 이 머신이 붙은 캐릭터. 이벤트 주체로 상태에 주입한다.
        private Animator _animator;
        public StateRunner CurrentState { get; private set; }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError($"[StateMachine] {name}에 Animator가 없습니다.");
            }

            _character = GetComponent<PlayerCharacter>();
            if (_character == null)
            {
                Debug.LogError($"[StateMachine] {name}에 PlayerCharacter가 없습니다.");
            }

            BuildStates();
        }

        private void OnEnable()
        {
            // 활성 머신만 트리거를 받도록 켜질 때 구독한다(스왑으로 꺼진 머신은 안 받음)
            _triggerSubscriber?.SubscribeTrigger(OnTrigger);

            // 활성화(스왑으로 켜질 때 포함)되면 Locomotion부터 시작
            TransitionTo(PlayerStateType.Locomotion);
        }

        private void OnDisable()
        {
            _triggerSubscriber?.UnsubscribeTrigger(OnTrigger);
        }

        // 채널로 들어온 외부 트리거를 상태 전환으로 넘긴다.
        private void OnTrigger(StateTriggerType trigger) => SendTrigger(trigger);

        private void Update()
        {
            if (CurrentState == null)
            {
                return;
            }
            
            CurrentState.Update();

            // 단발 애니가 끝났으면 종료 트리거(루프 상태는 제외)
            if (!CurrentState.IsLooping && CurrentState.IsAnimationFinished)
            {
                SendTrigger(StateTriggerType.AnimationFinished);
            }
        }

        // 상태 전환을 위한 트리거. 외부에서 상태를 전환시킬때 사용함
        public void SendTrigger(StateTriggerType trigger)
        {
            if (CurrentState == null)
            {
                return;
            }

            //대원_TODO: 상태 진행 중에 들어오는 트리거 입력은 즉각 수행되지 않고 저장되었다가 수행되도록 시스템 구현. 그게 없으면 즉각 호출됨.

            PlayerStateType? next = TryGetNextState(CurrentState.StateKey, trigger);
            if (next.HasValue)
            {
                TransitionTo(next.Value);
            }
        }

        // 왜: "어떤 상태에서 어떤 입력이 들어왔을 때 어떤 상태가 된다"를 나타냄. 전환 규칙을 한눈에 파악하기 위해 하드코딩으로 작성
        private PlayerStateType? TryGetNextState(PlayerStateType state, StateTriggerType trigger)
        {
            return (state, trigger) switch
            {
                (PlayerStateType.Dead, _) => null,

                // 경직(Hit)은 사망·애니종료로만 풀린다. 입력으로는 안 풀려서 "피격 우선"이 보장된다.
                (PlayerStateType.Hit, StateTriggerType.Died)              => PlayerStateType.Dead,
                (PlayerStateType.Hit, StateTriggerType.AnimationFinished) => PlayerStateType.Locomotion,
                (PlayerStateType.Hit, _)                                  => null,

                (PlayerStateType.NormalAttack1, StateTriggerType.Attack)      => PlayerStateType.NormalAttack2,
                (PlayerStateType.NormalAttack2, StateTriggerType.Attack)      => PlayerStateType.NormalAttack3,

                (_, StateTriggerType.AnimationFinished) => PlayerStateType.Locomotion,
                (_, StateTriggerType.Move)              => PlayerStateType.Locomotion,
                (_, StateTriggerType.Attack)            => PlayerStateType.NormalAttack1,
                (_, StateTriggerType.SpecialAttack)      => PlayerStateType.SpecialAttack,
                (_, StateTriggerType.FinishAttack)    => PlayerStateType.ExecutionAttack,
                (_, StateTriggerType.Dash)              => PlayerStateType.Dash,
                (_, StateTriggerType.LockOnDash)        => PlayerStateType.LockOnDash,
                (_, StateTriggerType.Damaged)           => PlayerStateType.Hit,
                (_, StateTriggerType.Died)              => PlayerStateType.Dead,

                _ => null,   
            };
        }
        private void BuildStates()
        {
            foreach (StateEntry entry in _stateEntries)
            {
                if(entry.data == null)
                {
                    Debug.LogWarning($"{entry.key}의 데이터가 존재하지 않습니다.");
                }            
                if(_animator != null && !_animator.HasState(0,Animator.StringToHash(entry.animationName)))
                {
                    Debug.LogWarning($"{entry.key}의 {entry.animationName}이 존재하지 않습니다.");
                }

                // 상태마다 클래스를 두지 않고, 단일 StateRunner를 키·데이터로 초기화해 등록한다. 락온 변형도 키별로 각각 등록.
                StateRunner state = new StateRunner();
                state.Initialize(entry.key, _character, _animator, _raiser, entry.data, entry.animationName);
                _states[entry.key] = state;
            }
        }

        private void TransitionTo(PlayerStateType next)
        {
            if (CurrentState != null && CurrentState.StateKey == next)
            {
                return; // 같은 상태로의 재진입은 무시(루프 유지·연타로 같은 상태 재시작 방지)
            }
            if (!_states.TryGetValue(next, out StateRunner nextState))
            {
                Debug.LogWarning($"[StateMachine] 등록되지 않은 상태로 전환 시도: {next}");
                return;
            }

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
        }
    }
}
