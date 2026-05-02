using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    [CustomPropertyDrawer(typeof(StateDataEntry))]
    public class StateDataEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float half = position.width / 2f;
            float spacing = 4f;

            Rect enumRect = new Rect(position.x, position.y, half/2f - spacing, position.height);
            Rect objRect = new Rect(position.x + half/2f + spacing, position.y, half * 1.5f - spacing, position.height);

            var stateTypeProp = property.FindPropertyRelative("stateType");
            var dataProp = property.FindPropertyRelative("data");

            EditorGUI.PropertyField(enumRect, stateTypeProp, GUIContent.none);
            EditorGUI.PropertyField(objRect, dataProp, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
