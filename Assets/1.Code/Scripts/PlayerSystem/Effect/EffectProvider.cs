using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Refactoring
{
    public class EffectProvider : MonoBehaviour, IEffectProvider
    {
        [Inject] private List<StateData> _stateDataList;
        private readonly Dictionary<object, Queue<GameObject>> _available = new();
        private readonly Dictionary<GameObject, object> _keyOfInstance = new(); //이펙트 반납시, 어디로 돌려보낼지를 위한 역맵핑
        private readonly HashSet<object> _requested = new(); //비동기 로딩 중복 방지용
        private readonly List<AsyncOperationHandle<GameObject>> _handles = new(); //비동기 로딩된 오브젝트 해제용
        private void Awake()
        {
            CollectAndLoad();
        }

        private void CollectAndLoad()
        {
            foreach (var data in _stateDataList)
            {
                var effectMap = data.GetData<SkillEffectDataEntry>();

                foreach (var entries in effectMap.Values)
                {
                    foreach (var entry in entries)
                    {
                        LoadAndBuild(entry.EffectObject);
                    }
                }
            }
        }

        private void LoadAndBuild(AssetReferenceGameObject asset)
        {
            if (asset == null || !asset.RuntimeKeyIsValid()) return;

            object key = asset.RuntimeKey;
            if (!_requested.Add(key)) return;

            var handle = Addressables.LoadAssetAsync<GameObject>(asset); 
            _handles.Add(handle);

            handle.Completed += op => 
            {
                if (op.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogWarning($"EffectProvider: 이펙트 로딩 실패 (key={key})");
                    return;
                }

                var queue = new Queue<GameObject>();
                for (int i = 0; i < 3; i++) //3개 이상이 필요해지면 이펙트 별로 여유분 수량 변수를 따로 만들기
                {
                    var instance = Instantiate(op.Result, transform); 
                    instance.SetActive(false);
                    queue.Enqueue(instance);
                    _keyOfInstance[instance] = key;
                }
                _available[key] = queue;
            };
        }

        public GameObject Rent(AssetReferenceGameObject key)
        {
            if (_available.TryGetValue(key.RuntimeKey, out var queue) && queue.Count > 0)
            {
                return queue.Dequeue();
            }
            return null;
        }

        public void Return(GameObject instance)
        {
            if (!_keyOfInstance.TryGetValue(instance, out var key))
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            _available[key].Enqueue(instance);
        }

        private void OnDestroy()
        {
            foreach (var instance in _keyOfInstance.Keys)
            {
                if (instance != null) Destroy(instance);
            }

            foreach (var handle in _handles)
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }
    }
}
