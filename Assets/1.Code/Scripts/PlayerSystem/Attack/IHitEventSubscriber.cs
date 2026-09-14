using System;

namespace Refactoring
{
    // 책임: 타격 성공 알림을 구독한다. (발행은 HitChannel.Raise, 구독자는 이 인터페이스만 본다)
    public interface IHitEventSubscriber
    {
        IDisposable Register(Action<HitReport> onHit);
    }
}
