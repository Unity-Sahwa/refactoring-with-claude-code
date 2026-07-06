using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // Project 창에서 에셋 우클릭 > "씬 사용처 찾기".
    // 선택한 에셋이 어느 씬에서, 어떤 중간 에셋(material > prefab 등)을 거쳐 쓰이는지 경로로 보여준다.
    // 어느 씬에도 안 쓰이면, 씬 밖의 다른 에셋이 물고 있는지까지 확인해 삭제 안전 여부를 알려준다.
    // 플레이 없이 에디터에서 동작. 직렬화 참조(드래그앤드롭/GUID) 기준이라 문자열 로드는 못 잡는다.
    public static class SceneUsageFinder
    {
        [MenuItem("Assets/씬 사용처 찾기")]
        private static void Find()
        {
            string target = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(target))
            {
                Debug.LogWarning("에셋을 하나 선택하세요.");
                return;
            }

            var scenes = AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath);
            var results = new List<string>();

            foreach (var scene in scenes)
            {
                // 1차 필터: 전체(연쇄) 의존성에 대상이 없으면 이 씬은 건너뜀
                if (!AssetDatabase.GetDependencies(scene, true).Contains(target)) continue;

                var path = FindPath(scene, target);
                if (path != null)
                {
                    results.Add(string.Join("\n→ ", path));
                }
            }

            if (results.Count > 0)
            {
                Debug.Log($"[{target}] 사용 경로 {results.Count}개:\n\n" + string.Join("\n\n", results));
                return;
            }

            // 씬엔 안 잡혔다. 씬 밖의 다른 에셋이 물고 있으면 삭제 시 그게 깨지므로 확인한다.
            var referrers = FindDirectReferrers(target);
            if (referrers.Count == 0)
            {
                Debug.Log($"[{target}]\n→ 어떤 씬에서도, 어떤 에셋에서도 안 쓰임 → 삭제해도 안전(직렬화 참조 기준).\n⚠ Resources.Load·Addressables·문자열 로드는 별도 확인 필요.");
            }
            else
            {
                Debug.LogWarning($"[{target}]\n→ 씬에는 안 쓰이지만 아래 에셋이 직접 참조 중 → 삭제하면 이들이 깨짐:\n→ " + string.Join("\n→ ", referrers));
            }
        }

        // 대상을 '직접' 의존성으로 물고 있는 에셋을 프로젝트 전체(Assets/)에서 찾는다.
        // 씬에 안 잡혀도 이들이 참조 중이면 삭제 시 깨지므로, 삭제 판단용으로만 쓴다.
        private static List<string> FindDirectReferrers(string target)
        {
            var referrers = new List<string>();
            var all = AssetDatabase.GetAllAssetPaths();
            try
            {
                for (int i = 0; i < all.Length; i++)
                {
                    var path = all[i];
                    if (path == target || !path.StartsWith("Assets/") || AssetDatabase.IsValidFolder(path)) continue;
                    if (EditorUtility.DisplayCancelableProgressBar("참조자 검색", path, (float)i / all.Length)) break;
                    if (AssetDatabase.GetDependencies(path, false).Contains(target)) referrers.Add(path);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            return referrers;
        }

        // 씬에서 대상까지 '직접 참조'만 따라가며 최단 경로 하나를 찾는다 (BFS).
        // ponytail: 노드마다 재귀 GetDependencies로 가지치기해 탐색을 줄임. 씬 트리가 매우 크면 느릴 수 있으나 단발 도구라 허용.
        private static List<string> FindPath(string scene, string target)
        {
            var parent = new Dictionary<string, string> { { scene, null } };
            var queue = new Queue<string>();
            queue.Enqueue(scene);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                foreach (var dep in AssetDatabase.GetDependencies(node, false))
                {
                    if (dep == node || parent.ContainsKey(dep)) continue;
                    parent[dep] = node;

                    if (dep == target)
                    {
                        return Rebuild(parent, target);
                    }
                    // 여전히 대상을 물고 있는 가지만 더 파고든다
                    if (AssetDatabase.GetDependencies(dep, true).Contains(target))
                    {
                        queue.Enqueue(dep);
                    }
                }
            }
            return null;
        }

        private static List<string> Rebuild(Dictionary<string, string> parent, string node)
        {
            var path = new List<string>();
            for (var n = node; n != null; n = parent[n])
            {
                path.Add(n);
            }
            path.Reverse();
            return path;
        }
    }
}
