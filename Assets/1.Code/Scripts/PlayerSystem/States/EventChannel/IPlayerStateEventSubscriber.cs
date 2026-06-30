using System;

namespace Refactoring
{
    public interface IPlayerStateEventSubscriber
    {
        public void SubscribeEnter(Action<PlayerCharacter, PlayerStateType> listener);
        public void UnsubscribeEnter(Action<PlayerCharacter, PlayerStateType> listener);

        public void Subscribe(StateEventCategory categoryType, Action<PlayerCharacter, IStartData> listener);
        public void Unsubscribe(StateEventCategory categoryType, Action<PlayerCharacter, IStartData> listener);
        
        public void SubscribeEnd(StateEventCategory categoryType, Action listener);
        public void UnsubscribeEnd(StateEventCategory categoryType, Action listener);
        
        public void SubscribeReset(Action listener);
        public void UnsubscribeReset(Action listener);
        
        
    }
}