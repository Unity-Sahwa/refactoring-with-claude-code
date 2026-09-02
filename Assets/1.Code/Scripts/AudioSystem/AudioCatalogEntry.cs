using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Refactoring
{
    [Serializable]
    public class AudioCatalogEntry
    {
        [SerializeField] private SoundType _id;

        [Tooltip("여럿이면 재생할 때 그 중 하나를 무작위로 고른다")]
        [SerializeField] private AudioClip[] _clips;

        [SerializeField] [Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private float _pitch = 1f;

        [Tooltip("0이면 2D(어디서나 같은 크기), 1이면 3D(거리에 따라 줄어듦)")]
        [SerializeField] [Range(0f, 1f)] private float _spatialBlend;

        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private float _maxDistance = 20f;
        [SerializeField] private bool _loop;

        [Tooltip("믹서 그룹에 속하여 볼륨설정에 영향을 받음")]
        [SerializeField] private AudioMixerGroup _output;

        public SoundType Id => _id;
        public AudioClip[] Clips => _clips;
        public float Volume => _volume;
        public float Pitch => _pitch;
        public float SpatialBlend => _spatialBlend;
        public float MinDistance => _minDistance;
        public float MaxDistance => _maxDistance;
        public bool Loop => _loop;
        public AudioMixerGroup Output => _output;
    }
}
