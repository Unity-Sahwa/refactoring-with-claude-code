using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // 역할: SO 에셋에서 UI 이펙트의 종류를 드롭다운으로 선택할 수 있도록 Editor로 편집
    [CustomPropertyDrawer(typeof(UIEffectConfig), true)]
    public class UIEffectConfigDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, line);
            Rect dropdownRect = new Rect(
                position.x + EditorGUIUtility.labelWidth + 2f,
                position.y,
                position.width - EditorGUIUtility.labelWidth - 2f,
                line);

            bool hasChildren = property.hasVisibleChildren;
            if (hasChildren)
            {
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            }
            else
            {
                EditorGUI.LabelField(labelRect, label);
            }

            string typeName = GetShortTypeName(property.managedReferenceFullTypename);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(typeName), FocusType.Keyboard))
            {
                BuildTypeMenu(property).DropDown(dropdownRect);
            }

            if (hasChildren && property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = position.y + line + spacing;
                SerializedProperty end = property.GetEndProperty();
                SerializedProperty child = property.Copy();
                bool enter = true;
                while (child.NextVisible(enter) && !SerializedProperty.EqualContents(child, end))
                {
                    enter = false;
                    float h = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), child, true);
                    y += h + spacing;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            float h = EditorGUIUtility.singleLineHeight;
            if (property.hasVisibleChildren && property.isExpanded)
            {
                float spacing = EditorGUIUtility.standardVerticalSpacing;
                SerializedProperty end = property.GetEndProperty();
                SerializedProperty child = property.Copy();
                bool enter = true;
                while (child.NextVisible(enter) && !SerializedProperty.EqualContents(child, end))
                {
                    enter = false;
                    h += EditorGUI.GetPropertyHeight(child, true) + spacing;
                }
            }
            return h;
        }

        private GenericMenu BuildTypeMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("(None)"), false, () => SetType(property, null));

            foreach (Type type in TypeCache.GetTypesDerivedFrom<UIEffectConfig>()
                         .Where(t => !t.IsAbstract && !t.IsInterface)
                         .OrderBy(t => t.Name))
            {
                Type captured = type;
                menu.AddItem(new GUIContent(captured.Name), false, () => SetType(property, captured));
            }
            return menu;
        }

        private void SetType(SerializedProperty property, Type type)
        {
            property.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);
            property.serializedObject.ApplyModifiedProperties();
        }

        private string GetShortTypeName(string managedReferenceFullTypename)
        {
            if (string.IsNullOrEmpty(managedReferenceFullTypename)) return "(None)";
            int space = managedReferenceFullTypename.IndexOf(' ');
            string typeName = space >= 0 ? managedReferenceFullTypename.Substring(space + 1) : managedReferenceFullTypename;
            int dot = typeName.LastIndexOf('.');
            return dot >= 0 ? typeName.Substring(dot + 1) : typeName;
        }
    }
}
