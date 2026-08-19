using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // ShakeEntry 리스트 항목 헤더를 "Element N" 대신 State 이름으로 보여준다.
    [CustomPropertyDrawer(typeof(ShakeEntry))]
    public class ShakeEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var state = property.FindPropertyRelative("State");
            label = new GUIContent(state.enumDisplayNames[state.enumValueIndex]); // State 이름을 헤더로
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);
    }
}
