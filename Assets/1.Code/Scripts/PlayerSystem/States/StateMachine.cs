using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 한 캐릭터의 상태 전환·구동을 담당한다. 캐릭터 GameObject에 붙는다.
    // 스왑은 GameObject 활성/비활성에 맡긴다 → 비활성이면 Unity가 Update를 안 부르므로 머신은 스왑을 모른다.
    // 흐름: 사건 → SendTrigger → 현재State.TryHandleTrigger → 전환 → Enter(애니 + 이벤트)
    public class StateMachine : MonoBehaviour
    {
        // 캐릭터별 상태 데이터 묶음(Inspector 드래그앤드롭). 클립명 + 이벤트 데이터.
        [Serializable]
        private class StateEntry
        {
            public PlayerStateType key;
            public string animationName;   // 캐릭터별 클립명(HIdle / AIdle 등)
            public BaseStateData data;     // 진행률 이벤트 데이터(SO)
        }

        [SerializeField] private List<StateEntry> _stateEntries = new List<StateEntry>(); // 데이터가 없어도 상태는 생성됨
        [Inject] private PlayerStateEventChannel _raiser;
        [Inject] private IInputEventProvider _input;
        [Inject(true)] private ILockOnState _lockOn;   // 외부 락온 시스템(없어도 동작)

        private readonly Dictionary<PlayerStateType, CharacterState> _states = new Dictionary<PlayerStateType, CharacterState>();
        private Animator _animator;
        private Vector2 _moveInput;   // OnMoveInput으로 갱신, Locomotion blend 입력에 사용
        public CharacterState CurrentState { get; private set; }
        public bool CanSwap => CurrentState != null && CurrentState.CanSwap;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError($"[StateMachine] {name}에 Animator가 없습니다.");
            }

            BuildStates();
        }

        private void OnEnable()
        {

            if (_input != null)
            {
                _input.OnInputPressed += HandleInput;
                _input.OnMoveInput += HandleMove;
            }

            // 활성화(스왑으로 켜질 때 포함)되면 Locomotion부터 시작
            TransitionTo(PlayerStateType.Locomotion);
            
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnInputPressed -= HandleInput;
                _input.OnMoveInput -= HandleMove;
            }
        }

        private void Update()
        {
            if (CurrentState == null)
            {
                return;
            }

            if (CurrentState is IMoveInputReceiver receiver)
            {
                receiver.SetMoveInput(_moveInput);
            }
            CurrentState.Update();

            // 단발 애니가 끝났으면 종료 트리거(루프 상태는 제외)
            if (!CurrentState.IsLooping && CurrentState.IsAnimationFinished)
            {
                SendTrigger(StateTriggerType.AnimationFinished);
            }
        }

        // 모든 사건의 단일 진입점(입력·피격·물리·애니완료)
        public void SendTrigger(StateTriggerType trigger)
        {
            if (CurrentState == null)
            {
                return;
            }

            if (CurrentState.TryHandleTrigger(trigger, out PlayerStateType next))
            {
                TransitionTo(next);
            }
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

                CharacterState state = CreateState(entry.key);
                if (state == null)
                {
                    continue;
                }

                state.Initialize(entry.key, _animator, _lockOn, _raiser, entry.data, entry.animationName);
                _states[entry.key] = state;
            }
        }

        // 명시적으로 상태 생성. 락온 변형은 같은 클래스를 공유한다.
        private CharacterState CreateState(PlayerStateType key)
        {
            switch (key)
            {
                case PlayerStateType.Locomotion:
                    return new LocomotionState();
                case PlayerStateType.Dash:
                case PlayerStateType.LockOnDash:
                    return new DashState();
                case PlayerStateType.NormalAttack1:
                    return new NormalAttack1State();
                case PlayerStateType.NormalAttack2:
                    return new NormalAttack2State();
                case PlayerStateType.NormalAttack3:
                    return new NormalAttack3State();
                case PlayerStateType.SpecialSkill:
                    return new SpecialSkillState();
                case PlayerStateType.ExecutionSkill:
                    return new ExecutionSkillState();
                case PlayerStateType.Jump:
                    return new JumpState();
                case PlayerStateType.Hit:
                    return new HitState();
                case PlayerStateType.Dead:
                    return new DeadState();
                default:
                    Debug.LogError($"[StateMachine] 알 수 없는 상태: {key}");
                    return null;
            }
        }

        private void TransitionTo(PlayerStateType next)
        {
            if (CurrentState != null && CurrentState.StateKey == next)
            {
                return; // 같은 상태면 무시
            }

            if (!_states.TryGetValue(next, out CharacterState nextState))
            {
                Debug.LogWarning($"[StateMachine] 등록되지 않은 상태로 전환 시도: {next}");
                return;
            }

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
        }

        // 입력 → 트리거 변환 (활성 머신만 구독)
        // 대원_TODO: 상태 의존 특수 콤보(예: 공격3 상태에서 공격 입력 → 스왑 + 특수공격)는
        //   입력 주체가 머신이므로 여기서 현재 상태(CurrentState.StateKey)를 보고
        //   ISwapInvoker.Swap(다음캐릭터, 시작트리거)로 처리한다.
        //   이런 특수 콤보가 여러 개로 늘어나면 플레이어 입력 커맨드 시스템으로 분리·발전 가능.
        private void HandleInput(InputActionType action)
        {
            switch (action)
            {
                case InputActionType.NormalAttack:
                    SendTrigger(StateTriggerType.Attack);
                    break;
                case InputActionType.SpecialAttack:
                    SendTrigger(StateTriggerType.SpecialSkill);
                    break;
                case InputActionType.FinishAttack:
                    SendTrigger(StateTriggerType.ExecutionSkill);
                    break;
                case InputActionType.Dash:
                    SendTrigger(StateTriggerType.Dash);
                    break;
                // LockOn은 상태 전환이 아니라 외부 락온 시스템이 처리 → 머신은 무시
                // Jump는 InputActionType에 아직 없음 → 입력 enum + .inputactions에 추가 필요
            }
        }

        private void HandleMove(Vector2 moveInput)
        {
            _moveInput = moveInput;   // 이동은 전환이 아니라 Locomotion blend 입력
        }
    }
}
