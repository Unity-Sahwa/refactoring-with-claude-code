using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // ObjectActiveEntry 한 줄을 Target 필드 + Active 체크박스로 나란히 그린다.
    [CustomPropertyDrawer(typeof(ObjectActiveEntry))]
    public class ObjectActiveEntryDrawer : PropertyDrawer
    {
        private const float CheckboxWidth = 24f;
        private const float Gap = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty target = property.FindPropertyRelative("Target");
            SerializedProperty active = property.FindPropertyRelative("Active");

            Rect activeRect = new Rect(position.x, position.y, CheckboxWidth, position.height);
            Rect targetRect = new Rect(activeRect.xMax + Gap, position.y, position.width - CheckboxWidth - Gap, position.height);

            EditorGUI.PropertyField(activeRect, active, GUIContent.none);
            EditorGUI.PropertyField(targetRect, target, GUIContent.none);
        }
    }
}
