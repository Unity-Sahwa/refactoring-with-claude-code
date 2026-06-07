using System;
using System.Collections.Generic;
using UnityEngine;


namespace Refactoring
{
    [CreateAssetMenu(menuName = "EventChannel/PlayerStateEventChennal")]
    public class PlayerStateEventChannel: ScriptableObject, IPlayerStateEventRaiser, IPlayerStateEventSubscriber
    {
        private Dictionary<StateEventCategory, Action<IStartData>> events = new();
        private Action _onResetEvent;

        void OnEnable()
        {
            foreach (StateEventCategory categoryType in Enum.GetValues(typeof(StateEventCategory)))
            {
                events[categoryType] = null;
            }
        }

        public void Raise(StateEventCategory categoryType, IStartData data)
        {
            events[categoryType]?.Invoke(data);
        }
             
        public void Subscribe(StateEventCategory categoryType, Action<IStartData> listener) 
        { 
            events[categoryType] += listener;
        }

        public void Unsubscribe(StateEventCategory categoryType, Action<IStartData> listener) 
        {
            events[categoryType] -= listener;
        }

        public void RaiseReset() => _onResetEvent?.Invoke();
        public void SubscribeReset(Action listener) => _onResetEvent += listener;
        public void UnsubscribeReset(Action listener) => _onResetEvent -= listener;
    }
}