using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Build Settings에 등록된 모든 씬을 훑어서 실제 쓰이는 머티리얼을 유니크하게 목록화.
/// 항목 클릭 시 Project 창에서 해당 에셋을 ping/선택. 값이 같은 중복 머티리얼은 뱃지로 표시.
/// ponytail: 씬을 순서대로 열었다 닫아 스캔(무거움). 자주 돌릴 툴이면 나중에 진행바/비동기 고려.
/// </summary>
public class MaterialDuplicateFinder : EditorWindow
{
    private List<Material> _materials = new();
    private Dictionary<Material, int> _duplicateGroup = new(); // material -> group index (같은 값끼리 묶임)
    private Vector2 _scroll;

    [MenuItem("Tools/씬 머티리얼 목록")]
    private static void Open()
    {
        GetWindow<MaterialDuplicateFinder>("씬 머티리얼 목록");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Build Settings 씬 전체 스캔", GUILayout.Height(28)))
            Scan();

        EditorGUILayout.LabelField($"머티리얼 {_materials.Count}개");
        EditorGUILayout.Space(4);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        foreach (var mat in _materials)
        {
            EditorGUILayout.BeginHorizontal("box");

            bool isDup = _duplicateGroup.TryGetValue(mat, out int group);
            if (isDup)
            {
                var prevColor = GUI.color;
                GUI.color = Color.yellow;
                GUILayout.Label($"중복#{group}", GUILayout.Width(60));
                GUI.color = prevColor;
            }
            else
            {
                GUILayout.Label("", GUILayout.Width(60));
            }

            if (GUILayout.Button(mat.name, EditorStyles.linkLabel, GUILayout.Width(200)))
            {
                EditorGUIUtility.PingObject(mat);
                Selection.activeObject = mat;
            }

            GUILayout.Label(AssetDatabase.GetAssetPath(mat), EditorStyles.miniLabel);

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void Scan()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return; // 저장 취소하면 스캔도 취소

        var originalSetup = EditorSceneManager.GetSceneManagerSetup();

        var materials = new List<Material>();
        foreach (var buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) continue;

            EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (var mat in renderers.SelectMany(r => r.sharedMaterials))
            {
                if (mat != null && !materials.Contains(mat))
                    materials.Add(mat);
            }
        }

        if (originalSetup.Length > 0)
            EditorSceneManager.RestoreSceneManagerSetup(originalSetup);

        _materials = materials.OrderBy(m => m.name).ToList();
        _duplicateGroup = BuildDuplicateGroups(_materials);
        Repaint();
    }

    // shader + 프로퍼티값이 같은 머티리얼끼리 그룹 번호를 매김 (1개짜리 그룹은 제외)
    private static Dictionary<Material, int> BuildDuplicateGroups(List<Material> materials)
    {
        var result = new Dictionary<Material, int>();
        int groupIndex = 0;
        foreach (var group in materials.GroupBy(BuildSignature))
        {
            var list = group.ToList();
            if (list.Count < 2) continue;
            groupIndex++;
            foreach (var mat in list)
                result[mat] = groupIndex;
        }
        return result;
    }

    private static string BuildSignature(Material mat)
    {
        var sb = new StringBuilder(mat.shader.name);
        var shader = mat.shader;
        int count = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < count; i++)
        {
            string propName = ShaderUtil.GetPropertyName(shader, i);
            var type = ShaderUtil.GetPropertyType(shader, i);
            sb.Append('|');
            switch (type)
            {
                case ShaderUtil.ShaderPropertyType.Color:
                    sb.Append(mat.GetColor(propName));
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                    sb.Append(mat.GetVector(propName));
                    break;
                case ShaderUtil.ShaderPropertyType.Float:
                case ShaderUtil.ShaderPropertyType.Range:
                    sb.Append(mat.GetFloat(propName));
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    var tex = mat.GetTexture(propName);
                    sb.Append(tex != null ? AssetDatabase.GetAssetPath(tex) : "null");
                    sb.Append(mat.GetTextureScale(propName));
                    sb.Append(mat.GetTextureOffset(propName));
                    break;
            }
        }
        return sb.ToString();
    }
}
