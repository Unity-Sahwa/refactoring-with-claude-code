using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 프리로드 때 미리 렌더해야 할 오브젝트(풀에 만들어 둔 이펙트, 보스 스킬 프리팹 등)를 넘겨준다.
    public interface IPreloadTargetProvider
    {
        IReadOnlyList<GameObject> PreloadTargets { get; }
    }
}
