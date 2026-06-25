namespace Refactoring
{
    // 외부(PlayerMovement 등)가 상태 전환 트리거를 쏘는 창구.
    // 받는 쪽(StateMachine)을 직접 모르고 채널에만 의존한다(단방향 결합).
    public interface IStateTriggerRaiser
    {
        public void RaiseTrigger(StateTriggerType trigger);
    }
}
