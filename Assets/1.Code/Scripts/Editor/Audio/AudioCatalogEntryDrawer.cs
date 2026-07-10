using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // AudioCatalog의 각 항목 헤더를 "Element N" 대신 그 항목의 id 이름으로 보여준다.
    [CustomPropertyDrawer(typeof(AudioCatalogEntry))]
    public class AudioCatalogEntryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var id = property.FindPropertyRelative("id");
            label = new GUIContent(id.enumDisplayNames[id.enumValueIndex]); // id 이름을 헤더로
            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);
    }
}