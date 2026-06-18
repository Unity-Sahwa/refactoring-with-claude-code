namespace Refactoring
{
    // 점프. 애니가 아니라 착지(물리 Landed 트리거)로 끝난다.
    // 점프 물리 처리는 진입 시 이벤트로 외부에 맡긴다.
    public class JumpState : CharacterState
    {
        public override bool TryHandleTrigger(StateTriggerType trigger, out PlayerStateType next)
        {
            if (trigger == StateTriggerType.Landed)
            {
                next = PlayerStateType.Locomotion;
                return true;
            }

            return base.TryHandleTrigger(trigger, out next);
        }
    }
}
