using System;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public class AudioDataEntry : IStartData
    {
        [SerializeField] private string name;
        [SerializeField] private AudioClip clip;
        [SerializeField] private bool loop;
        [SerializeField] private float volume;
        [SerializeField] private float pitch;
        [SerializeField] private float spatialBlend;
        [SerializeField] private float minDistance;
        [SerializeField] private float maxDistance;
        [SerializeField] [Range(0,1)] private float startProgress;
        [SerializeField] private float duration;

        public float StartProgress => startProgress;
    }
}
