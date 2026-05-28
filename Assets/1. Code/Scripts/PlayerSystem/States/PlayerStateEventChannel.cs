using System;
using System.Collections.Generic;
using UnityEngine;


namespace Refactoring
{
    [CreateAssetMenu(menuName = "EventChannel/PlayerStateEventChennal")]
    public class StateEventChannel: ScriptableObject, IStateEventRaiser, IStateEventSubscriber
    {
        private Dictionary<StateEventCategory, Action<int>> events = new();

        void OnEnable()
        {
            foreach (StateEventCategory categoryType in Enum.GetValues(typeof(StateEventCategory)))
            {
                events[categoryType] = null;
            }
        }

        public void Raise(StateEventCategory categoryType, int index)
        {
            events[categoryType]?.Invoke(index);
        }
             
        public void Subscribe(StateEventCategory categoryType, Action<int> listener) 
        { 
            events[categoryType] += listener;
        }

        public void Unsubscribe(StateEventCategory categoryType, Action<int> listener) 
        {
            events[categoryType] -= listener;
        }

        private Action _onResetEvent;
        public void RaiseReset() => _onResetEvent?.Invoke();
        public void SubscribeReset(Action listener) => _onResetEvent += listener;
        public void UnsubscribeReset(Action listener) => _onResetEvent -= listener;
    }
}