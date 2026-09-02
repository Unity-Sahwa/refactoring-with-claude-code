using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 상태가 발행한 기능 이벤트를 구독자에게 전달하는 채널.(발행자와 구독자는 서로를 모르고 채널을 통해서만 소식이 전달됨)
    // 사용: DataContainer에 데이터를 등록한다.
    [CreateAssetMenu(menuName = "EventChannel/PlayerStateEventChannel")]
    public class PlayerStateEventChannel : ScriptableObject, IPlayerStateEventRaiser, IPlayerStateEventSubscriber
    {
        private class StateEvent
        {
            public Action<IStartData> Open;
            public Action<CloseEventType> Close;
            public bool IsOpen;
        }

        private readonly Dictionary<StateEventCategory, List<StateEvent>> _switchEventMap = new();

        private void OnEnable()
        {
            _switchEventMap.Clear();
        }

        public void Raise(StateEventCategory category, IStartData data)
        {
            if (_switchEventMap.TryGetValue(category, out List<StateEvent> switchEvents))
            {
                for (int i = 0; i < switchEvents.Count; i++)
                {
                    // IsOpen을 켜는 이유는 End/Reset에서 "켜진 것만" 골라 닫기 위함.
                    switchEvents[i].IsOpen = true;
                    switchEvents[i].Open?.Invoke(data);
                }
            }
        }

        // 이벤트 지속시간 이후에 호출됨. 이벤트 잘 마무리되었다는 신호
        public void RaiseEnd(StateEventCategory category)
        {
            if (_switchEventMap.TryGetValue(category, out List<StateEvent> switchEvents))
            {
                CloseOpened(switchEvents, CloseEventType.End);
            }
        }

        // 상태 전환시 호출됨. 이벤트 초기화하라는 신호
        public void RaiseReset()
        {
            foreach (List<StateEvent> switchEvents in _switchEventMap.Values)
            {
                CloseOpened(switchEvents, CloseEventType.Reset);
            }
        }

        private void CloseOpened(List<StateEvent> events, CloseEventType reason)
        {
            for (int i = 0; i < events.Count; i++)
            {
                if (!events[i].IsOpen)
                {
                    continue;
                }
                events[i].IsOpen = false;
                events[i].Close?.Invoke(reason);
            }
        }

        // 구독자가 어떤 종류의 이벤트 알림을 받기 위한 용도. 
        // 동시에 알람 취소를 간단히 할 수 있게 IDisposable 반환
        public IDisposable Register(StateEventCategory category, Action<IStartData> open, Action<CloseEventType> close = null)
        {
            StateEvent eventSwitch = new StateEvent { Open = open, Close = close };

            if (!_switchEventMap.TryGetValue(category, out List<StateEvent> list))
            {
                list = new List<StateEvent>();
                _switchEventMap[category] = list;
            }
            list.Add(eventSwitch);

            return new DisposeAction(() => list.Remove(eventSwitch)); 
        }

        private class DisposeAction : IDisposable
        {
            private Action _dispose;

            // 대리자를 실행하는 것 외에는 아는 것이 없도록 함. 그저 대리자 실행만 할 뿐
            public DisposeAction(Action dispose) => _dispose = dispose;

            public void Dispose()
            {
                _dispose?.Invoke(); //list.Remove(eventSwitch) 실행됨. 더이상 알람 받지 않음.
                _dispose = null;
            }
        }
    }
}
