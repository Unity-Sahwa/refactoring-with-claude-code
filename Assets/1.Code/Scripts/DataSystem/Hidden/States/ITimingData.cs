using System;
using System.Collections.Generic;

namespace Refactoring
{
    public interface ITimingData<T> where T : Enum
    {
        public T StateType {get;}
        public Dictionary<StateEventCategory, List<IHasTimingData>> GetAllTimingData();
    }
}
