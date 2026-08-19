using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // ObjectToggleDataEntry의 각 항목 헤더를 "Element N" 대신 key 이름으로 보여준다.
    [CustomPropertyDrawer(typeof(ObjectToggleDataEntry))]
    public class ObjectToggleDataEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var key = property.FindPropertyRelative("key");
            label = new GUIContent(key.enumDisplayNames[key.enumValueIndex]); // key 이름을 헤더로
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);
    }
}
