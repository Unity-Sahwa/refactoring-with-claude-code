using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // 선택한 정적 오브젝트들(같은 머티리얼)을 메시 하나로 합쳐 드로우콜/셰도우캐스터 수를 줄인다
    public static class MeshCombinerTool
    {
        [MenuItem("Tools/Combine Selected Meshes")]
        private static void CombineSelectedMeshes()
        {
            MeshFilter[] meshFilters = Selection.gameObjects
                .Select(go => go.GetComponent<MeshFilter>())
                .Where(mf => mf != null && mf.sharedMesh != null)
                .ToArray();

            if (meshFilters.Length < 2)
            {
                Debug.LogWarning("MeshFilter를 가진 오브젝트를 2개 이상 선택하세요.");
                return;
            }

            Material sharedMaterial = meshFilters[0].GetComponent<MeshRenderer>()?.sharedMaterial;
            if (meshFilters.Any(mf => mf.GetComponent<MeshRenderer>()?.sharedMaterial != sharedMaterial))
            {
                Debug.LogWarning("선택한 오브젝트들의 머티리얼이 서로 다릅니다. 같은 머티리얼끼리만 결합 가능합니다.");
                return;
            }

            CombineInstance[] combineInstances = meshFilters.Select(mf => new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            }).ToArray();

            Mesh combinedMesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            combinedMesh.CombineMeshes(combineInstances);

            string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(meshFilters[0].sharedMesh));
            if (string.IsNullOrEmpty(folder) || !folder.Replace('\\', '/').StartsWith("Assets"))
                folder = "Assets";
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/Combined_{meshFilters[0].gameObject.name}.asset");
            AssetDatabase.CreateAsset(combinedMesh, assetPath);
            AssetDatabase.SaveAssets();

            GameObject combinedObject = new GameObject($"Combined_{meshFilters[0].gameObject.name}");
            combinedObject.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
            combinedObject.AddComponent<MeshRenderer>().sharedMaterial = sharedMaterial;
            combinedObject.isStatic = true;

            if (EditorUtility.DisplayDialog("결합 완료",
                    $"{meshFilters.Length}개 오브젝트를 결합했습니다. 원본 오브젝트를 삭제할까요?",
                    "삭제", "남겨두기"))
            {
                foreach (MeshFilter mf in meshFilters)
                    Undo.DestroyObjectImmediate(mf.gameObject);
            }

            Selection.activeGameObject = combinedObject;
            Debug.Log($"메시 결합 완료: {assetPath} (원본 {meshFilters.Length}개 → 1개)");
        }
    }
}
