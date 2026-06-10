using System;

namespace Refactoring
{
    public interface IPlayerStateEventRaiser
    {
        public void Raise(StateEventCategory categoryType, IStartData data);
        public void RaiseReset();
    }
}