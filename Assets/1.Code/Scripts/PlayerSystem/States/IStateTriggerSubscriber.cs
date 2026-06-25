using System;

namespace Refactoring
{
    // 상태 전환 트리거를 받는 쪽(StateMachine)이 구독하는 창구.
    // 쏘는 쪽(PlayerMovement 등)을 직접 모르고 채널에만 의존한다(단방향 결합).
    public interface IStateTriggerSubscriber
    {
        public void SubscribeTrigger(Action<StateTriggerType> listener);
        public void UnsubscribeTrigger(Action<StateTriggerType> listener);
    }
}
