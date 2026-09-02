using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 오디오 클립과 수치값을 한곳에 모아 id로 찾아준다.
    [CreateAssetMenu(menuName = "Refactoring/Audio/AudioCatalog")]
    public class AudioCatalog : ScriptableObject
    {
        [SerializeField] private AudioCatalogEntry[] _entries;

        private Dictionary<SoundType, AudioCatalogEntry> _map;

        public bool TryGet(SoundType id, out AudioCatalogEntry entry)
        {
            if (_map == null)
            {
                BuildMap();
            }

            return _map.TryGetValue(id, out entry);
        }

#if UNITY_EDITOR
        // 테스트시 수정되는 값을 바로 반영하기 위함.
        private void OnValidate()
        {
            BuildMap(); 
            WarnMissingIds();
        }

        // AudioId에는 있는데 _entries에 안 넣은 id를 찾는다.
        private void WarnMissingIds()
        {
            List<SoundType> missing = new();

            foreach (SoundType id in Enum.GetValues(typeof(SoundType)))
            {
                if (id == SoundType.None)
                {
                    continue;
                }

                if (!_map.ContainsKey(id))
                {
                    missing.Add(id);
                }
            }

            if (missing.Count > 0)
            {
                Debug.LogWarning($"[AudioCatalog] _entries에 없는 AudioId: {string.Join(", ", missing)}", this);
            }
        }
#endif

        private void BuildMap()
        {
            _map = new Dictionary<SoundType, AudioCatalogEntry>();

            if (_entries == null)
            {
                return;
            }

            foreach (AudioCatalogEntry entry in _entries)
            {
                if (entry == null || entry.Id == SoundType.None)
                {
                    continue;
                }

                _map[entry.Id] = entry;
            }
        }
    }
}
