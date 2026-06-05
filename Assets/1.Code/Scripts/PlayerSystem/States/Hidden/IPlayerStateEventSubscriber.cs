using System;

namespace Refactoring
{
    public interface IStateEventSubscriber
    {
        public void Subscribe(StateEventCategory categoryType, Action<int> listener);
        public void Unsubscribe(StateEventCategory categoryType, Action<int> listener);
        public void SubscribeReset(Action listener);
        public void UnsubscribeReset(Action listener);
    }
}