using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 시작 시 카탈로그의 히트박스 프리팹을 미리 복제하고, 히트박스를 대여/반납 형식으로 제공한다.
    public class HitboxProvider : MonoBehaviour, IHitboxProvider
    {
        [Inject] private HitboxCatalog _catalog;
        private readonly Dictionary<HitboxId, GameObject> _instances = new(); //키당 1개만 보관
        private readonly Dictionary<GameObject, MeshRenderer[]> _renderers = new(); //생성 시점에 캐싱한 MeshRenderer(런타임 순회 비용 제거)
        private readonly Dictionary<GameObject, IHitboxDamageReporter> _reporters = new(); //생성 시점에 캐싱한 감지 컴포넌트
        private void Awake()
        {
            BuildInstances();
        }

        private void BuildInstances()
        {
            foreach (var entry in _catalog.Entries)
            {
                if (entry == null || entry.Id == HitboxId.None || entry.Prefab == null) continue;
                if (_instances.ContainsKey(entry.Id)) continue; //같은 id 중복 방지

                var instance = Instantiate(entry.Prefab, transform); //여분 없이 키당 1개만 생성
                instance.SetActive(false);
                _renderers[instance] = instance.GetComponentsInChildren<MeshRenderer>(true); //비활성 자식까지 포함해 한 번만 수집
                _reporters[instance] = instance.GetComponentInChildren<IHitboxDamageReporter>(true); //감지 컴포넌트도 한 번만 수집
                _instances[entry.Id] = instance;
            }
        }

        public GameObject Rent(HitboxId id)
        {
            return _instances.TryGetValue(id, out var instance) ? instance : null;
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
        }
    }
}
