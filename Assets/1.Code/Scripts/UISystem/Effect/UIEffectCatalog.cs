using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 단순 연출을 위한 UI를 모은 카탈로그. 씬에 배치되는 UI는 따로 관리
    [CreateAssetMenu(menuName = "UISystem/UIEffectCatalog")]
    public class UIEffectCatalog : ScriptableObject
    {
        [SerializeField] private Entry[] entries;

        public IReadOnlyList<Entry> Entries => entries;

        [Serializable]
        public class Entry
        {
            [SerializeField] private UIEffectId id;
            [SerializeField] private GameObject prefab;
            [SerializeReference] private UIEffectConfig config; // 효과를 드롭다운(EffectConfigDrawer참고)으로 선택할 수 있음.

            public UIEffectId Id => id;
            public GameObject Prefab => prefab;
            public UIEffectConfig Config => config;
        }
    }
}
