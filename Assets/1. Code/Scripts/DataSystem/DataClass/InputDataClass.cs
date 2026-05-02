using System;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public class InputDataClass : IHasTimingData
    {
        [SerializeField] private string name;
        [SerializeField] [Range(0,1)] private float startProgress;
        [SerializeField] private float duration;

        public float StartProgress => startProgress;
        public float Duration => duration;
    }
}
