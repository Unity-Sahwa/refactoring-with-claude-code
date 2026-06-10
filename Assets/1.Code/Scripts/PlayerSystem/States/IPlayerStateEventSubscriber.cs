using System;

namespace Refactoring
{
    public interface IPlayerStateEventSubscriber
    {
        public void Subscribe(StateEventCategory categoryType, Action<IStartData> listener);
        public void Unsubscribe(StateEventCategory categoryType, Action<IStartData> listener);
        public void SubscribeReset(Action listener);
        public void UnsubscribeReset(Action listener);
    }
}