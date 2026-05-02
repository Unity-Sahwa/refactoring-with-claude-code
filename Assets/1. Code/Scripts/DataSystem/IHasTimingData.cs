using System.Collections;
using UnityEngine;

namespace Refactoring
{
    public interface IHasTimingData
    {
        public float StartProgress {get;}
        public float Duration {get;}

    }
}