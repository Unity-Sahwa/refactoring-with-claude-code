using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    //역할: 외부에서 이벤트 호출시 구독자에게 알림이 가도록하는 채널
    //      (호출자와 구독자는 서로를 모르고 채널을 통해서만 소식이 전달됨)
    // SOContainer에 등록해 [Inject]로 양쪽에 주입한다.
    [CreateAssetMenu(menuName = "EventChannel/PlayerStateEventChannel")]
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

        public void RaiseEnter(PlayerCharacter source, PlayerStateType state) 
        {
            _onEnter?.Invoke(source, state);
        }
        public void Raise(PlayerCharacter source, StateEventCategory categoryType, IStartData data)
        {
            events[categoryType]?.Invoke(source, data);
        }
        public void RaiseEnd(StateEventCategory categoryType)
        {
            endEvents[categoryType]?.Invoke();
        }
        public void RaiseReset()
        {
            _onResetEvent?.Invoke();  
        } 

        public void SubscribeEnter(Action<PlayerCharacter, PlayerStateType> listener)
        {
            _onEnter += listener;  
        } 
        public void Subscribe(StateEventCategory categoryType, Action<PlayerCharacter, IStartData> listener)
        {
            events[categoryType] += listener;
        }
        public void SubscribeEnd(StateEventCategory categoryType, Action listener)
        {
            endEvents[categoryType] += listener;
        }
        public void SubscribeReset(Action listener) 
        {
            _onResetEvent += listener;
        }

        public void UnsubscribeEnter(Action<PlayerCharacter, PlayerStateType> listener) 
        {
            _onEnter -= listener;
        }
        public void Unsubscribe(StateEventCategory categoryType, Action<PlayerCharacter, IStartData> listener)
        {
            events[categoryType] -= listener;
        }
        public void UnsubscribeEnd(StateEventCategory categoryType, Action listener)
        {
            endEvents[categoryType] -= listener;
        }
        public void UnsubscribeReset(Action listener) 
        {
            _onResetEvent -= listener;
        }
    }
}