namespace Refactoring
{
    // 책임: 타격 성공을 발행한다. (구독은 IHitEventSubscriber, 발행자는 이 인터페이스만 본다)
    public interface IHitEventRaiser
    {
        void Raise(HitReport report);
    }
}
