using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Refactoring
{
    // 번역 표. 키 하나에 한국어·영어 문장을 나란히 적어둔다.
    // 언어별 폰트도 여기 둔다. 글자마다 폰트를 꽂으면 배선이 너무 많아진다.
    [CreateAssetMenu(fileName = "TextTableData", menuName = "Refactoring/TextTableData")]
    public class TextTableData : ScriptableObject
    {
        [SerializeField]
        private List<Entry> _entries = new List<Entry>();

        [SerializeField]
        private TMP_FontAsset _koreanFont;

        [SerializeField]
        private TMP_FontAsset _englishFont;

        // 키로 빨리 찾으려고 한 번만 만들어 두는 색인.
        private Dictionary<string, Entry> _index;

        public List<Entry> Entries => _entries;

        public string GetText(string key, LanguageType language)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            BuildIndex();

            if (!_index.TryGetValue(key, out Entry entry))
            {
                Debug.LogWarning($"[TextTableData] 표에 없는 키: {key}");
                return key;
            }

            string text = language == LanguageType.English ? entry.English : entry.Korean;

            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning($"[TextTableData] {key}의 {language} 문장이 비어 있음");
                return key;
            }

            return text;
        }

        public TMP_FontAsset GetFont(LanguageType language)
        {
            return language == LanguageType.English ? _englishFont : _koreanFont;
        }

        // 드롭다운으로 키를 고를 때 쓴다.
        public List<string> GetKeys()
        {
            List<string> keys = new List<string>(_entries.Count);

            foreach (Entry entry in _entries)
            {
                keys.Add(entry.Key);
            }

            return keys;
        }

        // 표를 인스펙터나 도구로 고치면 색인을 새로 만들어야 한다.
        public void ClearIndex()
        {
            _index = null;
        }

        private void BuildIndex()
        {
            if (_index != null)
            {
                return;
            }

            _index = new Dictionary<string, Entry>(_entries.Count);

            foreach (Entry entry in _entries)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }

                _index[entry.Key] = entry;
            }
        }

        private void OnValidate()
        {
            ClearIndex();
        }

        // 표의 한 줄. 이 표에서만 쓴다.
        [Serializable]
        public class Entry
        {
            // public인 이유: 인스펙터에 그대로 노출되는 직렬화 값이라 프로퍼티로 감싸면 안 보인다.
            public string Key;

            [TextArea]
            public string Korean;

            [TextArea]
            public string English;
        }
    }
}
