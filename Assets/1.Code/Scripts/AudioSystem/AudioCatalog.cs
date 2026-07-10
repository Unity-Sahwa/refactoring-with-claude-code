using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 오디오 클립과 수치값을 한곳에 모음
    [CreateAssetMenu(menuName = "Audio/AudioCatalog")]
    public class AudioCatalog : ScriptableObject
    {
        [SerializeField] private AudioCatalogEntry[] entries;

        private Dictionary<AudioId, AudioCatalogEntry> _map;

        public bool TryGet(AudioId id, out AudioCatalogEntry entry)
        {
            if (_map == null) BuildMap();
            return _map.TryGetValue(id, out entry);
        }

        private void OnEnable() => BuildMap();

#if UNITY_EDITOR
        private void OnValidate() => BuildMap();
#endif

        private void BuildMap()
        {
            _map = new Dictionary<AudioId, AudioCatalogEntry>();
            if (entries == null) return;

            foreach (AudioCatalogEntry e in entries)
            {
                if (e == null || e.Id == AudioId.None) continue;
                _map[e.Id] = e; 
            }
        }
    }

    // 대부분 오디오 소스의 정보를 담음. 
    [Serializable]
    public class AudioCatalogEntry
    {
        [SerializeField] private AudioId id; //외부에서 아이디로 오디오를 요청함. 
        [SerializeField] private AudioClip[] clips;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private float pitch = 1f;
        [SerializeField] [Range(0f, 1f)] private float spatialBlend; // 0=2D, 1=3D
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private bool loop;

        public AudioId Id => id;
        public AudioClip[] Clips => clips;
        public float Volume => volume;
        public float Pitch => pitch;
        public float SpatialBlend => spatialBlend;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;
        public bool Loop => loop;
    }
}
