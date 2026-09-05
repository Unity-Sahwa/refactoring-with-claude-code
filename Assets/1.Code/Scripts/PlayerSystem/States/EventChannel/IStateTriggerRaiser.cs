namespace Refactoring
{
    // 책임: 받는 쪽에다가 trigger 타입으로 상태를 전환해달라는 트리거 호출.
    //      (받는 쪽을 직접 모르고 StateTriggerChannel에만 의존한다).
    public interface IStateTriggerRaiser
    {
        public void RaiseTrigger(StateTriggerType trigger);
    }
}
