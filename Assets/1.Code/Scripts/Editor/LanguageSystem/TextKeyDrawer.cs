using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Refactoring
{
    // [TextKey]가 붙은 문자열 칸을 드롭다운으로 그린다. 표에서 키 목록을 읽어온다.
    [CustomPropertyDrawer(typeof(TextKeyAttribute))]
    public class TextKeyDrawer : PropertyDrawer
    {
        private static string[] _allKeys;

        // 걸러낸 목록. 필드마다 드로어가 따로 만들어져서 여기 담아두면 필드별 목록이 된다.
        private string[] _keys;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            LoadKeys();
            FilterKeys();

            if (_keys.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            int index = System.Array.IndexOf(_keys, property.stringValue);

            // 표에 없는 키(예전 값이거나 오타)는 목록 맨 앞에 그대로 끼워 보여준다.
            if (index < 0)
            {
                List<string> withCurrent = new List<string> { $"(없는 키) {property.stringValue}" };
                withCurrent.AddRange(_keys);

                int picked = EditorGUI.Popup(position, label.text, 0, withCurrent.ToArray());

                if (picked > 0)
                {
                    property.stringValue = _keys[picked - 1];
                }

                return;
            }

            int newIndex = EditorGUI.Popup(position, label.text, index, _keys);
            property.stringValue = _keys[newIndex];
        }

        // 표는 보통 하나뿐이라 프로젝트에서 찾아 쓴다. 여러 개면 첫 번째를 쓴다.
        private static void LoadKeys()
        {
            if (_allKeys != null)
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:TextTableData");

            if (guids.Length == 0)
            {
                _allKeys = new string[0];
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            TextTableData table = AssetDatabase.LoadAssetAtPath<TextTableData>(path);

            _allKeys = table == null ? new string[0] : table.GetKeys().ToArray();
        }

        // [TextKey("Area")]처럼 걸러낼 글자를 줬으면 그 글자가 든 키만 남긴다.
        private void FilterKeys()
        {
            if (_keys != null)
            {
                return;
            }

            string filter = (attribute as TextKeyAttribute)?.Filter;

            if (string.IsNullOrEmpty(filter))
            {
                _keys = _allKeys;
                return;
            }

            List<string> picked = new List<string>();

            foreach (string key in _allKeys)
            {
                if (key.Contains(filter))
                {
                    picked.Add(key);
                }
            }

            _keys = picked.ToArray();
        }

        // 표를 고친 뒤 목록을 새로 읽게 한다.
        // 걸러낸 목록은 드로어마다 따로라 여기서 못 비운다. 인스펙터를 다시 그리면 새로 만들어진다.
        [MenuItem("Tools/언어/키 목록 새로고침")]
        private static void ClearCache()
        {
            _allKeys = null;
        }
    }
}
