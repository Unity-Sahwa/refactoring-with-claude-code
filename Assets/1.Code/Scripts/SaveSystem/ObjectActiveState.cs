using System;
using UnityEngine;

namespace Refactoring
{
    // 세이브 포인트가 기억해두는 오브젝트 하나의 온/오프 상태.
    // 이름이 아니라 계층 경로(부모까지 이어 붙인 이름)로 찾는다. 씬에 이름이 겹쳐도 부모가 다르면 구분된다.
    // 단, 같은 부모 밑에 이름이 같은 형제가 있으면 여전히 구분 못 한다.
    [Serializable]
    public struct ObjectActiveState
    {
        public string Path;
        public bool Active;

        public static string GetPath(Transform target)
        {
            string path = target.name;

            while (target.parent != null)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }

            return path;
        }

        // 비활성 오브젝트도 찾아야 해서 씬의 Transform을 전부 뒤진다.
        public static Transform Find(string path)
        {
            Transform[] all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Transform target in all)
            {
                if (GetPath(target) == path)
                {
                    return target;
                }
            }

            return null;
        }
    }
}
