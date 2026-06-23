using UnityEngine;

namespace Refactoring
{
    // 책임: 게임플레이 입력 중 "상태머신으로 가는 입력"만 전담한다(머신으로 들어가는 유일한 문).
    //  - InputBlock/InputBuffer 구간으로 막거나 저장하고,
    //  - 입력을 상태전환 트리거로 바꿔 머신에 발사한다(락온 여부에 따른 Dash 변형 포함).
    // 머신은 락온·block/buffer를 모른다(여기서 다 처리). 락온 값은 LockOnCamera(ILockOnState)에서 읽는다.
    // 흐름: 핸들러 OnInputPressed 구독 -> 전환 입력이면 block/buffer 판정 -> 트리거 변환 -> 머신 발사
    public class PlayerStateInputGate : MonoBehaviour
    {
        [Inject(true)] private IInputPressedProvider _inputProvider;     // 게임플레이 핸들러가 뿌리는 누르는 입력
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        [Inject(true)] private IStateTriggerRaiser _triggerRaiser;
        [Inject(true)] private ICharacterSwappable _characterSwitcher;
        [Inject(true)] private ILockOnState _lockOn;                     // 락온 여부(LockOnCamera 소유)

        private bool _inputBlock;
        private bool _buffering;
        private bool _hasBuffered;
        private InputActionType _bufferedAction;
        private PlayerStateType _currentState;   // 현재 상태(머신 Enter로 읽기만, NA3 스왑 해석용)

        private void Awake()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnInputPressed += OnPressed;
            }
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Subscribe(StateEventCategory.InputBlock, HandleBlockOn);
                _eventSubscriber.SubscribeEnd(StateEventCategory.InputBlock, HandleBlockOff);
                _eventSubscriber.Subscribe(StateEventCategory.InputBuffer, HandleBufferOn);
                _eventSubscriber.SubscribeEnd(StateEventCategory.InputBuffer, HandleBufferOff);
                _eventSubscriber.SubscribeReset(HandleReset);
                _eventSubscriber.SubscribeEnter(HandleEnter);
            }
        }

        private void OnDestroy()
        {
            if (_inputProvider != null)
            {
                _inputProvider.OnInputPressed -= OnPressed;
            }
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe(StateEventCategory.InputBlock, HandleBlockOn);
                _eventSubscriber.UnsubscribeEnd(StateEventCategory.InputBlock, HandleBlockOff);
                _eventSubscriber.Unsubscribe(StateEventCategory.InputBuffer, HandleBufferOn);
                _eventSubscriber.UnsubscribeEnd(StateEventCategory.InputBuffer, HandleBufferOff);
                _eventSubscriber.UnsubscribeReset(HandleReset);
                _eventSubscriber.UnsubscribeEnter(HandleEnter);
            }
        }

        // 핸들러가 뿌린 누르는 입력 처리. 전환 입력만 block/buffer 영향을 받고, 매핑 없는 입력(LockOn 등)은 그냥 흘려보낸다.
        private void OnPressed(InputActionType action)
        {
            if (ToTrigger(action).HasValue)
            {
                if (_inputBlock)
                {
                    return;
                }
                if (_buffering)
                {
                    _bufferedAction = action;
                    _hasBuffered = true;
                    return;
                }
            }

            SendInput(action);
        }

        // 저장된 버퍼 입력을 발사한다.
        private void SendBufferedInput()
        {
            if (!_hasBuffered) return;
            _hasBuffered = false;
            SendInput(_bufferedAction);
        }

        // 입력을 상태전환 트리거로 바꿔 머신에 쏜다. 즉시 발사·버퍼 발사 둘 다 여기를 거친다.
        private void SendInput(InputActionType action)
        {
            // NormalAttack3에서 기본공격 입력 → 스왑 후 특수 스킬(상태 의존 해석) 진행
            if (action == InputActionType.NormalAttack && _currentState == PlayerStateType.NormalAttack3)
            {
                _characterSwitcher?.SwapPlayerCharacter();
                _triggerRaiser?.RaiseTrigger(StateTriggerType.SpecialAttack);
                return;
            }

            StateTriggerType? trigger = ToTrigger(action);
            if (trigger.HasValue)
            {
                _triggerRaiser?.RaiseTrigger(trigger.Value);
            }
        }

        // 입력 액션 → 상태전환 트리거. 매핑 없는 입력(LockOn 등)은 null. Dash는 락온 여부로 변형.
        private StateTriggerType? ToTrigger(InputActionType action)
        {
            return action switch
            {
                InputActionType.NormalAttack  => StateTriggerType.Attack,
                InputActionType.SpecialAttack => StateTriggerType.SpecialAttack,
                InputActionType.FinishAttack  => StateTriggerType.FinishAttack,
                InputActionType.Dash          => _lockOn != null && _lockOn.IsLockOn ? StateTriggerType.LockOnDash : StateTriggerType.Dash,
                _ => null,
            };
        }

        private void HandleEnter(PlayerCharacter source, PlayerStateType state) => _currentState = state;
        private void HandleBlockOn(PlayerCharacter source, IStartData data) => _inputBlock = true;
        private void HandleBlockOff() => _inputBlock = false;
        private void HandleBufferOn(PlayerCharacter source, IStartData data) => _buffering = true;
        private void HandleBufferOff() //버퍼 구간이 끝날 때, 저장된 액션 호출
        {
            _buffering = false;
            SendBufferedInput();
        }
        private void HandleReset()
        {
            _inputBlock = false;
            _buffering = false;
            _hasBuffered = false;
        }
    }
}
