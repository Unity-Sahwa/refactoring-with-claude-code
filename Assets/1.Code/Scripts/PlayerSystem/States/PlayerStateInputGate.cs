using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 사용자 입력을 받아 PlayerStateMachine에 규칙에 따라 전달한다. 
    public class PlayerStateInputGate : MonoBehaviour
    {
        [Preserve, Inject(true)] private IInputPressedProvider _inputPressedProvider;
        [Preserve, Inject(true)] private IPlayerStateEventSubscriber _playerStateEventSubscriber;
        [Preserve, Inject(true)] private IStateTriggerRaiser _stateTriggerRaiser;
        [Preserve, Inject(true)] private ICharacterSwappable _characterSwitcher;
        [Preserve, Inject(true)] private ILockOnState _lockOnState;
        [Preserve, Inject(true)] private IFinishChecker _finishChecker;
        [Preserve, Inject(true)] private ICurrentStateProvider _currentStateProvider;

        private IDisposable _blockEventDisposable;
        private IDisposable _bufferEventDisposable;
        private bool _inputBlock;
        private bool _buffering;
        private bool _hasBuffered;
        private InputActionType _bufferedAction;

        private void Awake()
        {
            if (_inputPressedProvider != null)
            {
                _inputPressedProvider.OnInputPressed += OnPressed;
            }
            if (_playerStateEventSubscriber != null)
            {
                _blockEventDisposable = _playerStateEventSubscriber.Register(StateEventCategory.InputBlock, HandleBlockOn, HandleBlockClose);
                _bufferEventDisposable = _playerStateEventSubscriber.Register(StateEventCategory.InputBuffer, HandleBufferOn, HandleBufferClose);
            }
        }

        private void OnDestroy()
        {
            if (_inputPressedProvider != null)
            {
                _inputPressedProvider.OnInputPressed -= OnPressed;
            }
            _blockEventDisposable?.Dispose();
            _bufferEventDisposable?.Dispose();
        }

        // 외부 입력 이벤트 처리. 전환 입력만 block/buffer 영향을 받고, 매핑 없는 입력(LockOn 등)은 그냥 흘려보낸다.
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
            // NormalAttack3에서 기본공격 입력 → 스왑 후 특수 스킬(상태 의존 해석) 진행.
            // 현재 상태는 채널이 아니라 공유 SO(ICurrentStateProvider)에서 입력 시점에 조회한다.
            if (action == InputActionType.NormalAttack
                && _currentStateProvider != null && _currentStateProvider.CurrentState == PlayerStateType.NormalAttack3)
            {
                _characterSwitcher?.SwapPlayerCharacter();
                _stateTriggerRaiser?.RaiseTrigger(StateTriggerType.SpecialAttack);
                return;
            }

            // 처형 입력은 처형 가능한 대상이 있을 때만 상태로 보낸다(없으면 무시).
            if (action == InputActionType.FinishAttack && (_finishChecker == null || !_finishChecker.CanFinish()))
            {
                return;
            }

            StateTriggerType? trigger = ToTrigger(action);
            if (trigger.HasValue)
            {
                _stateTriggerRaiser?.RaiseTrigger(trigger.Value);
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
                InputActionType.Dash          => _lockOnState != null && _lockOnState.IsLockOn ? StateTriggerType.LockOnDash : StateTriggerType.Dash,
                _ => null,
            };
        }

        private void HandleBlockOn(IStartData data) => _inputBlock = true;
        // End든 Reset이든 입력 차단을 푸는 동작은 같다.
        private void HandleBlockClose(CloseEventType reason) => _inputBlock = false;
        private void HandleBufferOn(IStartData data) => _buffering = true;
        // End(구간 정상 종료): 저장된 입력을 발사. Reset(강제 이탈): 저장된 입력을 폐기.
        private void HandleBufferClose(CloseEventType reason)
        {
            _buffering = false;
            if (reason == CloseEventType.End)
            {
                SendBufferedInput();
            }
            else
            {
                _hasBuffered = false;
            }
        }
    }
}
