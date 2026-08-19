using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Refactoring
{
    public static class RemoveMissingScriptsTool
    {
        [MenuItem("Tools/Remove Missing Scripts In Scene")]
        private static void RemoveMissingScriptsInScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();
            int removedCount = 0;

            foreach (GameObject rootObject in rootObjects)
            {
                removedCount += RemoveMissingScriptsRecursive(rootObject);
            }

            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log($"Missing script {removedCount}개 제거 완료 (씬: {activeScene.name})");
        }

        // 자기 자신과 모든 자식 오브젝트를 순회하며 missing script를 제거한다
        private static int RemoveMissingScriptsRecursive(GameObject targetObject)
        {
            int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(targetObject);

            foreach (Transform child in targetObject.transform)
            {
                removedCount += RemoveMissingScriptsRecursive(child.gameObject);
            }

            return removedCount;
        }
    }
}
