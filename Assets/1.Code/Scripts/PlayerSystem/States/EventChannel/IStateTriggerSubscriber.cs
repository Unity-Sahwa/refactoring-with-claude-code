using System;

namespace Refactoring
{
    // 책임: 상태가 전환되는 트리거를 구독함.
    //       (트리거를 모르고 StateTriggerChannel 의존한다).
    public interface IStateTriggerSubscriber
    {
        public void SubscribeTrigger(Action<StateTriggerType> listener);
        public void UnsubscribeTrigger(Action<StateTriggerType> listener);
    }
}
