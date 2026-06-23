using System;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public class InputBlockDataEntry : IStartData
    {
        [SerializeField] private string name;
        [SerializeField] [Range(0, 1)] private float startProgress; 
        [SerializeField] [Range(0, 1)] private float duration;     
        [SerializeField] private bool untilEnd;                   

        //IStartData
        public float StartProgress => startProgress;

        public float Duration => duration;
        public bool UntilEnd => untilEnd;
    }
}
