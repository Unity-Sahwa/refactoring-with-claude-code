using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: EffectId → 프리팹·풀 크기 대응표를 한곳에 모은다. (프로바이더가 이걸 받아 시작 시 미리 복제해둔다)
    [CreateAssetMenu(menuName = "Effect/EffectCatalog")]
    public class EffectCatalog : ScriptableObject
    {
        [SerializeField] private EffectCatalogEntry[] _entries;

        public IReadOnlyList<EffectCatalogEntry> Entries => _entries;
    }
}
