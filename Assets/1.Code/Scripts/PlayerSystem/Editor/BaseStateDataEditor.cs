using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // BaseStateData(및 자식 SO) 인스펙터에 "빈 항목 숨기기" 토글을 추가한다.
    // 토글이 켜지면 비어있는 배열 필드는 그리지 않는다(토글 상태는 SessionState에 저장 → 에셋 오염 없음).
    [CustomEditor(typeof(BaseStateData), true)]
    public class BaseStateDataEditor : Editor
    {
        private const string HideEmptyKey = "BaseStateData.HideEmpty";

        public override void OnInspectorGUI()
        {
            bool hideEmpty = SessionState.GetBool(HideEmptyKey, false);
            bool newHideEmpty = EditorGUILayout.Toggle("빈 항목 숨기기", hideEmpty);
            if (newHideEmpty != hideEmpty)
            {
                SessionState.SetBool(HideEmptyKey, newHideEmpty);
                hideEmpty = newHideEmpty;
            }

            serializedObject.Update();

            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (prop.propertyPath == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.PropertyField(prop);
                    continue;
                }

                if (hideEmpty && IsEmptyArray(prop)) continue;

                EditorGUILayout.PropertyField(prop, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static bool IsEmptyArray(SerializedProperty prop)
        {
            // string도 isArray가 true라 제외한다.
            return prop.isArray
                && prop.propertyType != SerializedPropertyType.String
                && prop.arraySize == 0;
        }
    }
}
