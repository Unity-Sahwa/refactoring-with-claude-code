using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // 책임: 씬에 배치된 Canvas의 order를 볼 수 있다.(보기만 가능)
    // 흐름: 계층 바뀜 감지 → Collect(캔버스 모으고 정렬키 만들기) → OnGUI(그려지는 순서대로 위에서 아래로 표시)
    public class UIOrderViewer : EditorWindow
    {
        // 캔버스 하나를 표시하는 데 필요한 것만 미리 계산해서 담아둠. 정렬할 때마다 다시 계산 안 하려고.
        private readonly struct Entry
        {
            public readonly Canvas Canvas;
            public readonly string SortKey;
            public readonly string Tail;

            public Entry(Canvas canvas, string sortKey, string tail)
            {
                Canvas = canvas;
                SortKey = sortKey;
                Tail = tail;
            }
        }

        private readonly List<Entry> _entries = new();
        private Vector2 _scroll;

        [MenuItem("Tools/UI Order Viewer")]
        private static void Open()
        {
            GetWindow<UIOrderViewer>("UI Order");
        }

        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += Collect;
            Collect();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= Collect;
        }

        private void Collect()
        {
            _entries.Clear();

            // 꺼져 있는 캔버스도 순서에 끼어들 수 있어서 비활성 오브젝트까지 모은다.
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Canvas owner = SortingOwner(canvas);

                // 중첩 Canvas는 자기 renderMode 값이 무시되고 최상위 Canvas 걸 따라감. 그래서 rootCanvas에서 읽음.
                RenderMode mode = canvas.rootCanvas.renderMode;
                bool isOverlay = mode == RenderMode.ScreenSpaceOverlay;

                // Overlay는 카메라가 그린 결과 위에 무조건 덮어씌워짐. 그래서 뒷 그룹(1)에 둠.
                int group = isOverlay ? 1 : 0;
                // Overlay는 Sorting Layer를 안 봄. order 숫자만 봄.
                int layer = isOverlay ? 0 : SortingLayer.GetLayerValueFromID(owner.sortingLayerID);

                // 문자열 하나로 합쳐서 정렬키로 씀. 음수가 섞이니 10만을 더해 자릿수를 맞춤.
                string sortKey = $"{group}|{layer + 100000:D6}|{owner.sortingOrder + 100000:D6}|{HierarchyKey(canvas.transform)}";

                string tail;
                if (owner != canvas)
                {
                    tail = $"→ 부모({owner.name}) 순서 따라감";
                }
                else if (isOverlay)
                {
                    tail = $"[{mode}] order:{owner.sortingOrder}";
                }
                else
                {
                    tail = $"[{mode}] layer:{owner.sortingLayerName} order:{owner.sortingOrder}";
                }

                if (!canvas.gameObject.activeInHierarchy)
                {
                    tail += " (비활성)";
                }

                _entries.Add(new Entry(canvas, sortKey, tail));
            }

            _entries.Sort((a, b) => string.CompareOrdinal(a.SortKey, b.SortKey));
            Repaint();
        }

        // 자식 Canvas가 Override Sorting을 안 켰으면 순서는 부모 Canvas를 따라간다. 그 실제 주인을 찾는다.
        private static Canvas SortingOwner(Canvas canvas)
        {
            while (canvas.transform.parent != null && !canvas.overrideSorting)
            {
                Canvas parent = canvas.transform.parent.GetComponentInParent<Canvas>(true);
                if (parent == null)
                {
                    break;
                }

                canvas = parent;
            }

            return canvas;
        }

        // layer도 order도 같을 때 쓰는 마지막 기준. 계층에서 위에 있는 게 먼저 그려지므로 형제 번호를 위에서부터 이어붙임.
        private static string HierarchyKey(Transform transform)
        {
            string key = string.Empty;
            for (Transform t = transform; t != null; t = t.parent)
            {
                key = t.GetSiblingIndex().ToString("D5") + "/" + key;
            }

            return key;
        }

        private void OnGUI()
        {
            if (GUILayout.Button("새로고침"))
            {
                Collect();
            }

            EditorGUILayout.HelpBox("아래로 갈수록 나중에 그려짐 = 화면에서 위에 보임.", MessageType.None);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (Entry entry in _entries)
            {
                if (entry.Canvas == null)
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(entry.Canvas.gameObject, typeof(GameObject), true);
                EditorGUILayout.LabelField(entry.Tail, GUILayout.Width(260));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
