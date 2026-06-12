using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // [SerializeReference, SubclassSelector] 필드에 구현 타입 선택 드롭다운을 그린다.
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // managed reference가 아니면(=[SerializeReference]가 아니면) 기본 그리기
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // 선택된 타입의 필드들을 먼저 그린다.
            EditorGUI.PropertyField(position, property, label, true);

            // 헤더 줄 오른쪽에 타입 선택 드롭다운을 덮어 그린다.
            Rect dropdownRect = new Rect(
                position.x + EditorGUIUtility.labelWidth + 2f,
                position.y,
                position.width - EditorGUIUtility.labelWidth - 2f,
                EditorGUIUtility.singleLineHeight);

            string typeName = GetShortTypeName(property.managedReferenceFullTypename);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(typeName), FocusType.Keyboard))
            {
                BuildTypeMenu(property).DropDown(dropdownRect);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        // 필드(또는 리스트 요소)의 기준 타입을 구한다.
        private Type GetBaseType()
        {
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsArray) return fieldType.GetElementType();
            if (fieldType.IsGenericType) return fieldType.GetGenericArguments()[0];
            return fieldType;
        }

        private GenericMenu BuildTypeMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("(None)"), false, () => SetType(property, null));

            Type baseType = GetBaseType();
            if (baseType != null)
            {
                foreach (var type in TypeCache.GetTypesDerivedFrom(baseType)
                             .Where(t => !t.IsAbstract && !t.IsInterface)
                             .OrderBy(t => t.Name))
                {
                    Type captured = type;
                    menu.AddItem(new GUIContent(captured.Name), false, () => SetType(property, captured));
                }
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
            // "어셈블리이름 네임스페이스.타입이름" 형식에서 타입 이름만 뽑는다.
            int space = managedReferenceFullTypename.IndexOf(' ');
            string typeName = space >= 0 ? managedReferenceFullTypename.Substring(space + 1) : managedReferenceFullTypename;
            int dot = typeName.LastIndexOf('.');
            return dot >= 0 ? typeName.Substring(dot + 1) : typeName;
        }
    }
}
