using UnityEngine;

namespace Refactoring
{
    // 정지~이동 통합 상태(기존 Idle+Move). 블렌드트리로 idle~걷기를 입력 벡터로 blend한다.
    // 락온 여부는 별도 상태가 아니라 IsLockOn으로 blend 좌표만 분기한다. 루프 애니라 완료 전환 안 씀(IsLooping).
    // 실제 이동 처리는 외부(PlayerMovement)가 맡고, 여기선 blend 파라미터만 갱신한다.
    public class LocomotionState : CharacterState, IMoveInputReceiver
    {
        private static readonly int HorizontalHash = Animator.StringToHash("horizontal");
        private static readonly int VerticalHash = Animator.StringToHash("vertical");

        private Vector2 _moveInput;

        public override bool CanSwap => true;
        public override bool IsLooping => true;

        // 머신이 매 프레임 Update 전에 입력을 넣어준다(IMoveInputReceiver).
        public void SetMoveInput(Vector2 moveInput) => _moveInput = moveInput;

        public override void Update()
        {
            base.Update();   // 진행률 이벤트(데이터가 있으면)

            // 비락온: 전진만(방향은 외부 회전이 처리) / 락온: 입력 그대로 스트레이프(타겟 기준 orbit)
            if (IsLockOn())
            {
                Animator.SetFloat(HorizontalHash, _moveInput.x);
                Animator.SetFloat(VerticalHash, _moveInput.y);
            }
            else
            {
                Animator.SetFloat(HorizontalHash, 0f);
                Animator.SetFloat(VerticalHash, _moveInput.magnitude);
            }
        }

        public override bool TryHandleTrigger(StateTriggerType trigger, out PlayerStateType next)
        {
            switch (trigger)
            {
                case StateTriggerType.Attack:
                    next = PlayerStateType.NormalAttack1;
                    return true;
                case StateTriggerType.Dash:
                    next = IsLockOn() ? PlayerStateType.LockOnDash : PlayerStateType.Dash;
                    return true;
                case StateTriggerType.Jump:
                    next = PlayerStateType.Jump;
                    return true;
                case StateTriggerType.SpecialSkill:
                    next = PlayerStateType.SpecialSkill;
                    return true;
                case StateTriggerType.ExecutionSkill:
                    next = PlayerStateType.ExecutionSkill;
                    return true;
                default:
                    return base.TryHandleTrigger(trigger, out next);
            }
        }

        private bool IsLockOn() => LockOn != null && LockOn.IsLockOn;
    }
}
