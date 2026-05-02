using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    //상태 매니저에서 얘만 가져가기. 
    public interface ITimingData
    {
        public Dictionary<StateDataCategoryType, List<IHasTimingData>> GetAllTimingData();
    }
}
