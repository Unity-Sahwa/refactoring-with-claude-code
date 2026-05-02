using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshCombiner : MonoBehaviour
{
    [SerializeField] private GameObject[] targets;

    private void Start()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            CombineMeshes(targets[i]);
            targets[i].SetActive(false);
        }
    }

    public static GameObject CombineMeshes(GameObject target) // 메쉬들을 하나로 병합하는 함수
    {
        // 현재 오브젝트 하위에 포함된 모든 MeshRenderer와 MeshFilter를 저장한다.
        List<MeshRenderer> rendererList = new List<MeshRenderer>();
        List<MeshFilter> filterList = new List<MeshFilter>();

        Transform[] childs = GetAllChild(target.transform);
        for (int i = 0, l = childs.Length; i < l; ++i)
        {
            MeshRenderer renderer = childs[i].GetComponent<MeshRenderer>();
            MeshFilter mesh = childs[i].GetComponent<MeshFilter>();

            if (renderer && renderer.sharedMaterials.Length > 0 && mesh && mesh.sharedMesh)
            {
                rendererList.Add(renderer);
                filterList.Add(mesh);
            }
        }

        // 머티리얼로 메쉬 데이터(Mesh와 로컬 Matrix)들을 저장하는 Dictionary를 구성한다.
        // MeshFilter의 매쉬를 GetSubMeshes 함수를 통해 서브 메쉬(머티리얼을 바탕으로 구분된 메쉬)로 나누어 저장한다.
        // 따라서, 같은 머티리얼이 여러 오브젝트에 나뉘어 사용되어도 병합할 수 있다.
        Dictionary<Material, List<Tuple<Mesh, Matrix4x4>>> mat2Mesh = new Dictionary<Material, List<Tuple<Mesh, Matrix4x4>>>();
        for (int i = 0, l = filterList.Count; i < l; ++i)
        {
            Mesh[] meshes = GetSubMeshes(filterList[i].sharedMesh);
            Material[] materials = rendererList[i].sharedMaterials;
            for (int j = 0, l2 = materials.Length; j < l2; ++j)
            {
                if (!materials[j]) continue;

                if (!mat2Mesh.ContainsKey(materials[j]))
                    mat2Mesh.Add(materials[j], new List<Tuple<Mesh, Matrix4x4>>());

                mat2Mesh[materials[j]].Add(Tuple.Create(meshes[j], filterList[i].transform.localToWorldMatrix));
            }
        }

        // 메쉬들을 병합하고 새 오브젝트를 구성한다.
        // CombineInstance에 메쉬와 트랜스폼을 저장하고 CombineMeshes 함수에 넘겨주면 하나의 메쉬로 병합할 수 있다.
        // 버텍스 개수가 65535개를 벗어나면 포맷을 변경해주어야 오류를 방지할 수 있다.
        GameObject combinedTarget = new GameObject(target.name + " (Combined)");
        foreach (Material mat in mat2Mesh.Keys)
        {
            int vertexCount = 0;
            List<Tuple<Mesh, Matrix4x4>> meshDatas = mat2Mesh[mat];
            CombineInstance[] combines = new CombineInstance[meshDatas.Count];
            for (int i = 0, l = meshDatas.Count; i < l; ++i)
            {
                Mesh mesh = meshDatas[i].Item1;
                combines[i].mesh = mesh;
                combines[i].transform = meshDatas[i].Item2;
                vertexCount += mesh.vertexCount;
            }

            GameObject child = new GameObject(mat.name);
            child.transform.SetParent(combinedTarget.transform, false);
            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            MeshFilter filter = child.AddComponent<MeshFilter>();

            filter.mesh = new Mesh();
            filter.mesh.indexFormat = vertexCount > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            filter.mesh.CombineMeshes(combines);
            renderer.material = mat;
        }

        return combinedTarget;
    }
    
    public static Transform[] GetAllChild(Transform target) // 모든 자식을 반환하는 함수
    {
        // 자식 오브젝트를 재귀 탐색한다.
        List<Transform> childs = new List<Transform>();

        childs.Add(target);
        for (int i = 0, l = target.childCount; i < l; ++i)
            childs.AddRange(GetAllChild(target.GetChild(i)));

        return childs.ToArray();
    }
    public static Mesh[] GetSubMeshes(Mesh mesh) // 서브 메쉬들을 반환하는 함수
    {
        if (mesh == null) return null;

        // 서브 메쉬의 개수만큼 반환할 메쉬 배열을 생성한다.
        // 현재 메쉬의 정점, UV, 노멀 정보를 저장해둔다.
        int subMeshCount = mesh.subMeshCount;
        Mesh[] subMeshes = new Mesh[subMeshCount];

        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        Vector3[] normals = mesh.normals;

        for (int i = 0, l = subMeshCount; i < l; ++i)
        {
            // 새로운 메쉬를 만들기 위한 리스트들을 생성해둔다.
            List<Vector3> newVertices = new List<Vector3>();
            List<Vector2> newUVs = new List<Vector2>();
            List<Vector3> newNormals = new List<Vector3>();
            List<int> newTriangles = new List<int>();

            // 현재 서브 메쉬의 삼각형 정보를 담는다.
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0, l2 = triangles.Length; j < l2; j += 3)
            {
                // 삼각형 세 개의 인덱스로 리스트들을 구성한다.
                int idx = triangles[j];
                int idx2 = triangles[j + 1];
                int idx3 = triangles[j + 2];

                newVertices.Add(vertices[idx]);
                newVertices.Add(vertices[idx2]);
                newVertices.Add(vertices[idx3]);

                newUVs.Add(uvs[idx]);
                newUVs.Add(uvs[idx2]);
                newUVs.Add(uvs[idx3]);

                newNormals.Add(normals[idx]);
                newNormals.Add(normals[idx2]);
                newNormals.Add(normals[idx3]);

                newTriangles.Add(newTriangles.Count);
                newTriangles.Add(newTriangles.Count);
                newTriangles.Add(newTriangles.Count);
            }

            // 서브 메쉬를 생성하고 리스트 정보들를 반영한다.
            subMeshes[i] = new Mesh();
            subMeshes[i].indexFormat = newVertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;

            subMeshes[i].vertices = newVertices.ToArray();
            subMeshes[i].uv = newUVs.ToArray();
            subMeshes[i].normals = newNormals.ToArray();
            subMeshes[i].triangles = newTriangles.ToArray();
        }

        return subMeshes;
    }
  
}
