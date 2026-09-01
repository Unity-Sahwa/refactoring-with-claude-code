using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 프리로드로 미리 렌더할 배경/몬스터 오브젝트에 붙이는 표식.
    // 붙이기만 하면 AttributeInjector가 비활성 상태까지 훑어 EventPreloadRender에 넘긴다.
    public class PreloadTarget : MonoBehaviour, IPreloadTargetProvider
    {
        private GameObject[] _self;

        public IReadOnlyList<GameObject> PreloadTargets => _self ??= new[] { gameObject };
    }
}
