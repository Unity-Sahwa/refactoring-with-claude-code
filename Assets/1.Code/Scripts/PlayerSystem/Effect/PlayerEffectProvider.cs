using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 시작 시 카탈로그의 이펙트 프리팹을 미리 복제해두고, 이펙트를 대여/반납 형식으로 제공한다.(활성화 가능한 이펙트만 대여한다.)
    public class PlayerEffectProvider : MonoBehaviour, IEffectProvider
    {
        [Inject] private EffectCatalog _catalog;
        private readonly Dictionary<EffectId, Queue<GameObject>> _available = new();
        private readonly Dictionary<GameObject, EffectId> _keyOfInstance = new(); //이펙트 반납시, 어디로 돌려보낼지를 위한 역맵핑
        private void Awake()
        {
            BuildPools();
        }

        private void BuildPools()
        {
            foreach (var entry in _catalog.Entries)
            {
                if (entry == null || entry.Id == EffectId.None || entry.Prefab == null) continue;
                if (_available.ContainsKey(entry.Id)) continue; //같은 id 중복 방지

                var queue = new Queue<GameObject>();
                int count = Mathf.Max(1, entry.PoolSize);
                for (int i = 0; i < count; i++)
                {
                    var instance = Instantiate(entry.Prefab, transform);
                    instance.SetActive(false);
                    queue.Enqueue(instance);
                    _keyOfInstance[instance] = entry.Id;
                }
                _available[entry.Id] = queue;
            }
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
                if (instance != null) Destroy(instance);
            }
        }
    }
}
