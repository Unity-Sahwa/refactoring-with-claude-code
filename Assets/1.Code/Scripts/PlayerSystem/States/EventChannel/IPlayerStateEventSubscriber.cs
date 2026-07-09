using System;

namespace Refactoring
{
    public interface IPlayerStateEventSubscriber
    {
        IDisposable Register(StateEventCategory category, Action<IStartData> open, Action<CloseEventType> close = null);
    }
}
