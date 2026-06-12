using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Refactoring
{
    public class HitboxProvider : MonoBehaviour, IHitboxProvider, IDataInjectable
    {
        public Dictionary<Type, List<ScriptableObject>> RequiredData { get; } = new()
        {
            { typeof(BaseStateData), new List<ScriptableObject>() }
        };

        private readonly Dictionary<object, GameObject> _instances = new(); //키당 1개만 보관
        private readonly Dictionary<GameObject, MeshRenderer[]> _renderers = new(); //생성 시점에 캐싱한 MeshRenderer(런타임 순회 비용 제거)
        private readonly Dictionary<GameObject, IHitboxDamageReporter> _reporters = new(); //생성 시점에 캐싱한 감지 컴포넌트
        private readonly HashSet<object> _requested = new(); //비동기 로딩 중복 방지용
        private readonly List<AsyncOperationHandle<GameObject>> _handles = new(); //비동기 로딩된 오브젝트 해제용
        private void Awake()
        {
            CollectAndLoad();
        }

        private void CollectAndLoad()
        {
            foreach (var so in RequiredData[typeof(BaseStateData)])
            {
                var data = (BaseStateData)so;
                var hitboxMap = data.GetData<HitboxDataEntry>();

                foreach (var entries in hitboxMap.Values)
                {
                    foreach (var entry in entries)
                    {
                        LoadAndBuild(entry.HitboxObject);
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
                    Debug.LogWarning($"HitboxProvider: 히트박스 로딩 실패 (key={key})");
                    return;
                }

                var instance = Instantiate(op.Result, transform); //여분 없이 키당 1개만 생성
                instance.SetActive(false);
                _renderers[instance] = instance.GetComponentsInChildren<MeshRenderer>(true); //비활성 자식까지 포함해 한 번만 수집
                _reporters[instance] = instance.GetComponentInChildren<IHitboxDamageReporter>(true); //감지 컴포넌트도 한 번만 수집
                _instances[key] = instance;
            };
        }

        public GameObject Rent(AssetReferenceGameObject key)
        {
            return _instances.TryGetValue(key.RuntimeKey, out var instance) ? instance : null;
        }

        public void SetMeshVisible(GameObject instance, bool visible)
        {
            if (!_renderers.TryGetValue(instance, out var renderers)) return;
            foreach (var renderer in renderers)
            {
                if (renderer != null) renderer.enabled = visible;
            }
        }

        public IHitboxDamageReporter GetReporter(GameObject instance)
        {
            return _reporters.TryGetValue(instance, out var reporter) ? reporter : null;
        }

        public void Return(GameObject instance)
        {
            if (!_instances.ContainsValue(instance)) //우리가 만든 인스턴스가 아니면 풀에 넣지 않는다
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
        }

        private void OnDestroy()
        {
            foreach (var instance in _instances.Values)
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
