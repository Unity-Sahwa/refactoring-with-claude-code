using System;

namespace Refactoring
{
    public interface IStateEventRaiser
    {
        public void Raise(StateEventCategory categoryType, int index);
        public void RaiseReset();
    }
}

