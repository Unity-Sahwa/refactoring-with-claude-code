using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 씬에서 못 잡는 SO를 모아 DI에 넘긴다. 그룹은 인스펙터 정리용일 뿐 주입에는 영향이 없다.
    public class DataContainer : MonoBehaviour, IDataProvider
    {
        [SerializeField] private List<DataGroup> _groups = new List<DataGroup>();

        public List<ScriptableObject> ProvideData()
        {
            List<ScriptableObject> all = new List<ScriptableObject>();

            foreach (DataGroup group in _groups)
            {
                all.AddRange(group.Assets);
            }

            return all;
        }

        // 첫 필드가 string이면 인스펙터가 그 값을 요소 이름으로 쓴다. 그래서 Name을 맨 위에 둔다.
        [Serializable]
        private class DataGroup
        {
            // 인스펙터 직렬화 대상이라 public 필드여야 한다.
            public string Name;
            public List<ScriptableObject> Assets = new List<ScriptableObject>();
        }
    }
}
