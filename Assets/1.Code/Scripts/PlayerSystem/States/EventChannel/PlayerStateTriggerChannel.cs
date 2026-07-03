using System;
using UnityEngine;

namespace Refactoring
{
    // 역할: 상태 전환 트리거 전용 이벤트 채널(SO). 쏘는 쪽과 받는 쪽이 서로를 모르고 이 채널만 공유한다.
    // SOContainer에 등록해 [Inject]로 양쪽에 주입한다.
    [CreateAssetMenu(menuName = "EventChannel/PlayerStateTriggerChannel")]
    public class PlayerStateTriggerChannel : ScriptableObject, IStateTriggerRaiser, IStateTriggerSubscriber
    {
        private Action<StateTriggerType> _onTrigger;

        public void RaiseTrigger(StateTriggerType trigger) 
        {
            _onTrigger?.Invoke(trigger);
        }

        public void SubscribeTrigger(Action<StateTriggerType> listener)
        {
            _onTrigger += listener;
        } 

        public void UnsubscribeTrigger(Action<StateTriggerType> listener) 
        {
            _onTrigger -= listener;
        }

    }
}
