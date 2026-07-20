using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: HitboxId → 프리팹 대응표를 한곳에 모음.(AudioCatalog와 같은 방식)
    //       프로바이더가 이걸 받아 시작 시 프리팹을 미리 복제해둔다.
    [CreateAssetMenu(menuName = "Hitbox/HitboxCatalog")]
    public class HitboxCatalog : ScriptableObject
    {
        [SerializeField] private HitboxCatalogEntry[] entries;

        public IReadOnlyList<HitboxCatalogEntry> Entries => entries;
    }

    // 히트박스 하나에 대한 정의.(히트박스는 키당 1개라 풀 크기 없음)
    [Serializable]
    public class HitboxCatalogEntry
    {
        [SerializeField] private HitboxId id;         // 외부에서 이 id로 히트박스를 요청함.
        [SerializeField] private GameObject prefab;   // 그 id가 쓸 실제 프리팹.

        public HitboxId Id => id;
        public GameObject Prefab => prefab;
    }
}
