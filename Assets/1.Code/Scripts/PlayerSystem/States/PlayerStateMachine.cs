using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 캐릭터에 붙어서 한 번에 하나의 상태만 진행되도록 관리. 규칙에 따른 상태 직접 전환
    public class PlayerStateMachine : MonoBehaviour, IDataProvider
    {
        [SerializeField] private List<StateData> _stateDataList = new List<StateData>();
        [Preserve, Inject] private IPlayerStateEventRaiser _raiser;
        [Preserve, Inject] private IStateTriggerSubscriber _triggerSubscriber;
        [Preserve, Inject(true)] private ICurrentStateWriter _currentStateWriter;

        private readonly Dictionary<PlayerStateType, StateRunner> _states = new Dictionary<PlayerStateType, StateRunner>();
        private readonly Dictionary<PlayerStateType, float> _lastEnterTime = new Dictionary<PlayerStateType, float>();
        private PlayerCharacter _character;
        public StateRunner CurrentState { get; private set; }

        private void Awake()
        {
            _character = GetComponent<PlayerCharacter>();
            if (_character == null)
            {
                Debug.LogError($"[StateMachine] {name}에 PlayerCharacter가 없습니다.");
                return;
            }

            BuildStates();
        }

        private void OnEnable()
        {
            _triggerSubscriber?.SubscribeTrigger(OnTrigger);
            // 스왑으로 다시 활성화될 때도 "현재 활성 상태"가 공유 SO에 최신으로 반영되게 한 번 기록한다.
            if (CurrentState != null)
            {
                _currentStateWriter?.SetCurrentState(CurrentState.StateKey);
            }
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

            if (!CurrentState.IsLooping && CurrentState.IsAnimationFinished)
            {
                SendTrigger(StateTriggerType.AnimationFinished);
            }
        }

        // 내외부의 신호에 따라 상태를 전환시키는 트리거 
        public void SendTrigger(StateTriggerType trigger)
        {
            if (CurrentState == null)
            {
                return;
            }

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

                (PlayerStateType.Hit, StateTriggerType.Died)              => PlayerStateType.Dead,
                (PlayerStateType.Hit, StateTriggerType.AnimationFinished) => PlayerStateType.Locomotion,

                (PlayerStateType.NormalAttack1, StateTriggerType.Attack)      => PlayerStateType.NormalAttack2,
                (PlayerStateType.NormalAttack2, StateTriggerType.Attack)      => PlayerStateType.NormalAttack3,

                (_, StateTriggerType.AnimationFinished) => PlayerStateType.Locomotion,
                (_, StateTriggerType.Move)              => PlayerStateType.Locomotion,
                (_, StateTriggerType.Attack)            => PlayerStateType.NormalAttack1,
                (_, StateTriggerType.SpecialAttack)      => PlayerStateType.SpecialAttack,
                (_, StateTriggerType.FinishAttack)    => PlayerStateType.FinishAttack,
                (_, StateTriggerType.Dash)              => PlayerStateType.FrontDash,
                (_, StateTriggerType.LockOnDash)        => PlayerStateType.BackDash,
                (_, StateTriggerType.Damaged)           => PlayerStateType.Hit,
                (_, StateTriggerType.Died)              => PlayerStateType.Dead,

                _ => null,   
            };
        }
        private void BuildStates()
        {
            foreach (StateData data in _stateDataList)
            {
                if (data == null)
                {
                    Debug.LogWarning("[StateMachine] 비어있는 상태 데이터 항목이 있습니다.");
                    continue;
                }

                StateRunner state = new StateRunner();
                state.Initialize(_character, _raiser, data);
                _states[data.StateType] = state;
            }
            
            CurrentState = _states.GetValueOrDefault(PlayerStateType.Locomotion) ?? _states.Values.First();
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

            // 상태별 쿨타임: 마지막으로 이 상태에 들어간 뒤 설정된 시간이 안 지났으면 전환 무시 (스킬 연타 방지)
            if (_lastEnterTime.TryGetValue(next, out float lastTime) && Time.time - lastTime < nextState.Cooldown)
            {
                return;
            }

            CurrentState?.Exit();
            CurrentState = nextState;
            _lastEnterTime[next] = Time.time;
            _currentStateWriter?.SetCurrentState(next);
            CurrentState.Enter();
        }

        public List<ScriptableObject> ProvideData()
        {
            return _stateDataList.ConvertAll(d => (ScriptableObject)d);
        }
    }
}
