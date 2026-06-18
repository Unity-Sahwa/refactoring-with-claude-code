namespace Refactoring
{
    // 사망(최종 상태). 더 이상 어떤 트리거에도 전환하지 않는다.
    public class DeadState : CharacterState
    {
        public override bool TryHandleTrigger(StateTriggerType trigger, out PlayerStateType next)
        {
            next = default;
            return false;
        }
    }
}
