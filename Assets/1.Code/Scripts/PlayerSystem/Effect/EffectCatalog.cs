using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: EffectId → 프리팹 + 풀 크기 대응표를 한곳에 모음.(AudioCatalog와 같은 방식)
    //       프로바이더가 이걸 받아 시작 시 프리팹을 미리 복제해둔다.
    [CreateAssetMenu(menuName = "Effect/EffectCatalog")]
    public class EffectCatalog : ScriptableObject
    {
        [SerializeField] private EffectCatalogEntry[] entries;

        public IReadOnlyList<EffectCatalogEntry> Entries => entries;
    }

    // 이펙트 하나에 대한 정의.
    [Serializable]
    public class EffectCatalogEntry
    {
        [SerializeField] private EffectId id;         // 외부에서 이 id로 이펙트를 요청함.
        [SerializeField] private GameObject prefab;   // 그 id가 쓸 실제 프리팹.
        [SerializeField] private int poolSize = 3;    // 미리 복제해둘 여유분 개수.

        public EffectId Id => id;
        public GameObject Prefab => prefab;
        public int PoolSize => poolSize;
    }
}
