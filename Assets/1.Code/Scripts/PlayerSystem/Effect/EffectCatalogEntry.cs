using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 이펙트 하나의 정의(id·프리팹·풀 크기).
    [Serializable]
    public class EffectCatalogEntry
    {
        [Tooltip("외부에서 이 id로 이펙트를 요청한다")]
        [SerializeField] private EffectId _id;
        [Tooltip("그 id가 쓸 실제 프리팹")]
        [SerializeField] private GameObject _prefab;
        [Tooltip("미리 복제해둘 여유분 개수")]
        [SerializeField] private int _poolSize = 3;

        public EffectId Id => _id;
        public GameObject Prefab => _prefab;
        public int PoolSize => _poolSize;
    }
}
