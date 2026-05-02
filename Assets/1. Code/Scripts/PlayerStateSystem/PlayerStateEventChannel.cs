using System;
using System.Collections.Generic;
using UnityEngine;


namespace Refactoring
{
    [CreateAssetMenu(menuName = "EventChannel/PlayerStateEventChennal")]
    public class PlayerStateEventChannel: ScriptableObject
    {
        private Dictionary<StateDataCategoryType, Action<object>> events = new();

        void OnEnable()
        {
            foreach (StateDataCategoryType categoryType in Enum.GetValues(typeof(StateDataCategoryType)))
            {
                events[categoryType] = null;
            }
        }

        public void Raise(StateDataCategoryType categoryType, object data)
        {
            events[categoryType]?.Invoke(data);
            Debug.Log($"Raise: {categoryType}");
        }
             
        public void Subscribe(StateDataCategoryType categoryType, Action<object> listener) 
        { 
            events[categoryType] += listener;
            Debug.Log($"Subscribe: {categoryType}");
        }

        public void Unsubscribe(StateDataCategoryType categoryType, Action<object> listener) 
        {
            events[categoryType] -= listener;
            Debug.Log($"Unsubscribe: {categoryType}");
        }


        private Action _onResetEvent;
        public void RaiseReset() => _onResetEvent?.Invoke();
        public void SubscribeReset(Action listener) => _onResetEvent += listener;
        public void UnsubscribeReset(Action listener) => _onResetEvent -= listener;
    }
}