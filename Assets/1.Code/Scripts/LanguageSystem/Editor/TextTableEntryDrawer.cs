using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // 표 한 줄을 접지 않고 [키][한국어][영어]로 나란히 그린다.
    // 줄마다 토글을 열어야 하면 90줄을 훑어보기가 너무 불편하다.
    [CustomPropertyDrawer(typeof(TextTableData.Entry))]
    public class TextTableEntryDrawer : PropertyDrawer
    {
        private const float Gap = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty key = property.FindPropertyRelative("Key");
            SerializedProperty korean = property.FindPropertyRelative("Korean");
            SerializedProperty english = property.FindPropertyRelative("English");

            float usable = position.width - Gap * 2f;
            float keyWidth = usable * 0.3f;
            float textWidth = (usable - keyWidth) * 0.5f;

            Rect keyRect = new Rect(position.x, position.y, keyWidth, EditorGUIUtility.singleLineHeight);
            Rect koreanRect = new Rect(keyRect.xMax + Gap, position.y, textWidth, keyRect.height);
            Rect englishRect = new Rect(koreanRect.xMax + Gap, position.y, textWidth, keyRect.height);

            // 라벨 없이 칸만 그린다. 어느 칸인지는 자리로 구분한다.
            key.stringValue = EditorGUI.TextField(keyRect, key.stringValue);
            korean.stringValue = EditorGUI.TextField(koreanRect, korean.stringValue);
            english.stringValue = EditorGUI.TextField(englishRect, english.stringValue);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
