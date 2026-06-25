using System;
using System.Collections.Generic;
using UnityEngine;


namespace Refactoring
{
    [CreateAssetMenu(menuName = "EventChannel/PlayerStateEventChennal")]
    public class PlayerStateEventChannel: ScriptableObject, IPlayerStateEventRaiser, IPlayerStateEventSubscriber
    {
        private Dictionary<StateEventCategory, Action<PlayerCharacter, IStartData>> events = new();
        private Dictionary<StateEventCategory, Action> endEvents = new();   // 구간 끝(끄기)
        private Action _onResetEvent;
        private Action<PlayerCharacter, PlayerStateType> _onEnter;

        void OnEnable()
        {
            foreach (StateEventCategory categoryType in Enum.GetValues(typeof(StateEventCategory)))
            {
                events[categoryType] = null;
                endEvents[categoryType] = null;
            }
        }

        public void Raise(PlayerCharacter source, StateEventCategory categoryType, IStartData data)
        {
            events[categoryType]?.Invoke(source, data);
        }
        public void RaiseEnd(StateEventCategory categoryType)
        {
            endEvents[categoryType]?.Invoke();
        }
        public void Subscribe(StateEventCategory categoryType, Action<PlayerCharacter, IStartData> listener)
        {
            events[categoryType] += listener;
        }
        public void Unsubscribe(StateEventCategory categoryType, Action<PlayerCharacter, IStartData> listener)
        {
            events[categoryType] -= listener;
        }
        public void SubscribeEnd(StateEventCategory categoryType, Action listener)
        {
            endEvents[categoryType] += listener;
        }
        public void UnsubscribeEnd(StateEventCategory categoryType, Action listener)
        {
            endEvents[categoryType] -= listener;
        }

        public void RaiseReset() => _onResetEvent?.Invoke();
        public void SubscribeReset(Action listener) => _onResetEvent += listener;
        public void UnsubscribeReset(Action listener) => _onResetEvent -= listener;

        public void RaiseEnter(PlayerCharacter source, PlayerStateType state) => _onEnter?.Invoke(source, state);
        public void SubscribeEnter(Action<PlayerCharacter, PlayerStateType> listener) => _onEnter += listener;
        public void UnsubscribeEnter(Action<PlayerCharacter, PlayerStateType> listener) => _onEnter -= listener;
    }
}