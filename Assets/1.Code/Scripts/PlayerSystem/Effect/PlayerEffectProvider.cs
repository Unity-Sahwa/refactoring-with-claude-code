using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 시작 시 카탈로그의 이펙트 프리팹을 미리 복제해두고, 이펙트를 대여/반납 형식으로 제공한다.(활성화 가능한 이펙트만 대여한다.)
    public class PlayerEffectProvider : MonoBehaviour, IEffectProvider, IPreloadTargetProvider
    {
        [Preserve, Inject] private EffectCatalog _catalog;
        private readonly Dictionary<EffectId, Queue<GameObject>> _available = new();
        // 반납된 이펙트를 어느 풀로 돌려보낼지 찾기 위한 역맵
        private readonly Dictionary<GameObject, EffectId> _keyOfInstance = new();
        private readonly List<GameObject> _preloadTargets = new();

        // 프리로드가 미리 렌더할 대상. 만들어 둔 풀 인스턴스를 그대로 넘긴다.
        public IReadOnlyList<GameObject> PreloadTargets => _preloadTargets;

        private void Awake()
        {
            if (_catalog == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerEffectProvider)}: 필수 의존 주입 실패");
            }

            BuildPools();
        }

        private void BuildPools()
        {
            foreach (var entry in _catalog.Entries)
            {
                if (!IsPoolableEntry(entry))
                {
                    continue;
                }

                BuildPool(entry);
            }
        }

        private bool IsPoolableEntry(EffectCatalogEntry entry)
        {
            if (entry == null || entry.Id == EffectId.None || entry.Prefab == null)
            {
                return false;
            }

            // 같은 id 중복 방지
            return !_available.ContainsKey(entry.Id);
        }

        private void BuildPool(EffectCatalogEntry entry)
        {
            var queue = new Queue<GameObject>();
            int count = Mathf.Max(1, entry.PoolSize);
            for (int i = 0; i < count; i++)
            {
                GameObject instance = Instantiate(entry.Prefab, transform);
                instance.SetActive(false);
                queue.Enqueue(instance);
                _keyOfInstance[instance] = entry.Id;
                _preloadTargets.Add(instance);
            }
            _available[entry.Id] = queue;
        }

        public GameObject Rent(EffectId id)
        {
            if (_available.TryGetValue(id, out var queue))
            {
                if (queue.Count > 0)
                {
                    return queue.Dequeue();
                }

                Debug.LogWarning($"EffectProvider: 풀 고갈 (id={id}). 여유분 늘리기 검토");
                return null;
            }
            return null;
        }

        public void Return(GameObject instance)
        {
            if (!_keyOfInstance.TryGetValue(instance, out var id))
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            _available[id].Enqueue(instance);
        }

        private void OnDestroy()
        {
            foreach (var instance in _keyOfInstance.Keys)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
        }
    }
}
