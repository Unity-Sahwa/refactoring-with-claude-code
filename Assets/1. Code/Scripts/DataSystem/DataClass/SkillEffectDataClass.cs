using System;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public class SkillEffectDataClass : IHasTimingData
    {
        [SerializeField] private string name;
        [SerializeField] private bool untilFinish;
        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private Vector3 scale;
        [SerializeField] [Range(0,1)] private float startProgress;
        [SerializeField] private float duration;

        public float StartProgress => startProgress;
        public float Duration => duration;

    }
}