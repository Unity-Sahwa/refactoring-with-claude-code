using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Refactoring
{
    // 지정한 타입의 컴포넌트를 여러 씬에서 한번에 찾아 인스펙터 값을 표로 보고 수정한다.
    // 스캔 대상 씬은 자동으로 열었다가(Additive) 저장 후 닫는다 — 원래 열려있던 씬은 건드리지 않는다.
    // ponytail: 배열/리스트 필드는 표 레이아웃이 복잡해져 편집 대상에서 제외. 단일 값 필드만 지원.
    public class SceneComponentBatchEditor : EditorWindow
    {
        private class ComponentRow
        {
            public string ScenePath;
            public string ObjectPath;
            public SerializedObject SerializedObject;
        }

        private MonoScript _targetScript;
        private Type _targetType;
        private bool _scanAllProjectScenes;

        private readonly List<ComponentRow> _rows = new List<ComponentRow>();
        private readonly List<string> _fieldNames = new List<string>();
        private readonly HashSet<string> _openedScenePaths = new HashSet<string>();

        private Vector2 _scrollPosition;

        [MenuItem("Tools/씬 컴포넌트 일괄 편집기")]
        private static void Open()
        {
            GetWindow<SceneComponentBatchEditor>("씬 컴포넌트 일괄 편집기");
        }

        private void OnGUI()
        {
            DrawScanControls();
            EditorGUILayout.Space();
            DrawTable();
        }

        private void DrawScanControls()
        {
            EditorGUI.BeginChangeCheck();
            _targetScript = (MonoScript)EditorGUILayout.ObjectField("대상 스크립트", _targetScript, typeof(MonoScript), false);
            if (EditorGUI.EndChangeCheck())
            {
                _targetType = _targetScript != null ? _targetScript.GetClass() : null;
            }

            _scanAllProjectScenes = EditorGUILayout.ToggleLeft("Build Settings 대신 프로젝트 전체 씬 스캔", _scanAllProjectScenes);

            using (new EditorGUI.DisabledScope(_targetType == null))
            {
                if (GUILayout.Button("스캔"))
                {
                    Scan();
                }
            }

            using (new EditorGUI.DisabledScope(_rows.Count == 0))
            {
                if (GUILayout.Button("저장 후 닫기"))
                {
                    SaveAndClose();
                }
            }
        }

        // 씬을 열어 대상 타입의 컴포넌트를 전부 찾고, 표의 열(필드 목록)을 첫 인스턴스 기준으로 확정한다
        private void Scan()
        {
            SaveAndClose();

            IEnumerable<string> scenePaths = _scanAllProjectScenes
                ? AssetDatabase.FindAssets("t:Scene").Select(AssetDatabase.GUIDToAssetPath)
                : EditorBuildSettings.scenes.Select(sceneAsset => sceneAsset.path);

            foreach (string scenePath in scenePaths)
            {
                Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
                bool wasAlreadyLoaded = scene.IsValid() && scene.isLoaded;
                if (!wasAlreadyLoaded)
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    _openedScenePaths.Add(scenePath);
                }

                foreach (GameObject rootObject in scene.GetRootGameObjects())
                {
                    foreach (Component component in rootObject.GetComponentsInChildren(_targetType, true))
                    {
                        _rows.Add(new ComponentRow
                        {
                            ScenePath = scenePath,
                            ObjectPath = GetHierarchyPath(component.transform),
                            SerializedObject = new SerializedObject(component),
                        });
                    }
                }
            }

            if (_rows.Count > 0)
            {
                CollectFieldNames(_rows[0].SerializedObject);
            }

            Debug.Log($"[씬 컴포넌트 일괄 편집기] {_targetType.Name} {_rows.Count}개 발견");
        }

        // 배열이 아닌 SerializeField만 열로 사용한다 (m_Script 제외)
        private void CollectFieldNames(SerializedObject sample)
        {
            _fieldNames.Clear();
            SerializedProperty iterator = sample.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script" || iterator.isArray)
                {
                    continue;
                }
                _fieldNames.Add(iterator.name);
            }
        }

        private void DrawTable()
        {
            if (_rows.Count == 0)
            {
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("오브젝트", EditorStyles.boldLabel, GUILayout.Width(220));
            foreach (string fieldName in _fieldNames)
            {
                GUILayout.Label(fieldName, EditorStyles.boldLabel, GUILayout.Width(150));
            }
            EditorGUILayout.EndHorizontal();

            foreach (ComponentRow row in _rows)
            {
                row.SerializedObject.Update();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(row.ObjectPath, GUILayout.Width(220));

                EditorGUI.BeginChangeCheck();
                foreach (string fieldName in _fieldNames)
                {
                    SerializedProperty property = row.SerializedObject.FindProperty(fieldName);
                    EditorGUILayout.PropertyField(property, GUIContent.none, GUILayout.Width(150));
                }
                if (EditorGUI.EndChangeCheck())
                {
                    row.SerializedObject.ApplyModifiedProperties();
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetSceneByPath(row.ScenePath));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // 이 도구가 연 씬만 저장 후 닫는다 (원래 열려있던 씬은 그대로 둔다)
        private void SaveAndClose()
        {
            foreach (string scenePath in _openedScenePaths)
            {
                Scene scene = EditorSceneManager.GetSceneByPath(scenePath);
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            _openedScenePaths.Clear();
            _rows.Clear();
            _fieldNames.Clear();
        }

        private static string GetHierarchyPath(Transform target)
        {
            string path = target.name;
            for (Transform parent = target.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }
            return path;
        }
    }
}
