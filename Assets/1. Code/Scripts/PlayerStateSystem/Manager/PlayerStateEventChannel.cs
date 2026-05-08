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
            IHasTimingData test = (IHasTimingData)data;
            //Debug.Log($"Raise categoryType: {categoryType}");
            //Debug.Log($"Raise StartProgress: {test.StartProgress}");

            events[categoryType]?.Invoke(data);
        }
             
        public void Subscribe(StateDataCategoryType categoryType, Action<object> listener) 
        { 
            Debug.Log($"Subscribe categoryType: {categoryType}");
            events[categoryType] += listener;
        }

        public void Unsubscribe(StateDataCategoryType categoryType, Action<object> listener) 
        {
            Debug.Log($"Unsubscribe categoryType: {categoryType}");
            events[categoryType] -= listener;
        }


        private Action _onResetEvent;
        public void RaiseReset() => _onResetEvent?.Invoke();
        public void SubscribeReset(Action listener) => _onResetEvent += listener;
        public void UnsubscribeReset(Action listener) => _onResetEvent -= listener;
    }
}