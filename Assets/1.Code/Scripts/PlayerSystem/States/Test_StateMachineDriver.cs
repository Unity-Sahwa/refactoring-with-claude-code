using UnityEngine;

namespace Refactoring
{
    // 테스트용: 외부 시스템(피격/물리/락온)이 머신을 DI로 받아 SendTrigger를 호출하는 실제 패턴을 흉내낸다.
    //   머신·입력 모두 의존성 주입으로 받고, 실제 입력 이벤트(IInputEventProvider)로 외부 트리거를 쏜다.
    //   락온은 이 컴포넌트가 ILockOnState로 제공한다(머신이 [Inject(true)]로 주입받음).
    // 입력 매핑 (InputActionType에 피격/점프 액션이 없어 남는 액션에 토글로 묶음):
    //   LockOn      = 락온 모드 토글
    //   Interaction = 피격(Damaged) / Hit 상태에서 다시 누르면 사망(Died)
    //   Menu        = 점프(Jump) / Jump 상태에서 다시 누르면 착지(Landed)
    // 공격·대쉬·스킬은 머신이 직접 입력을 받아 처리하므로 여기서 다루지 않는다.
    public class Test_StateMachineDriver : MonoBehaviour, ILockOnState
    {
        [Inject] private StateMachine _machine;
        [Inject] private IInputEventProvider _input;

        private bool _isLockOn;
        public bool IsLockOn => _isLockOn;

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnInputPressed += HandleInput;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnInputPressed -= HandleInput;
            }
        }

        private void HandleInput(InputActionType action)
        {
            if (_machine == null || _machine.CurrentState == null)
            {
                return;
            }

            PlayerStateType current = _machine.CurrentState.StateKey;

            switch (action)
            {
                case InputActionType.LockOn:
                    _isLockOn = !_isLockOn;
                    Debug.Log($"[Test_StateMachineDriver] 락온: {_isLockOn}");
                    break;

                case InputActionType.Interaction:
                    _machine.SendTrigger(current == PlayerStateType.Hit ? StateTriggerType.Died : StateTriggerType.Damaged);
                    break;

                case InputActionType.Menu:
                    _machine.SendTrigger(current == PlayerStateType.Jump ? StateTriggerType.Landed : StateTriggerType.Jump);
                    break;
            }
        }
    }
}
