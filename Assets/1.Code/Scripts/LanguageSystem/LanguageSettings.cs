using System;
using TMPro;
using UnityEngine;

namespace Refactoring
{
    // 지금 언어 창구. 설정창이 바꾸고, 화면의 글자들이 읽고 알림을 듣는다.
    public interface ILanguageSettings
    {
        event Action OnChanged;

        Language Current { get; set; }

        string GetText(string key);
        TMP_FontAsset GetFont();
    }

    [Serializable]
    public class LanguageSettingsData : ISaveData
    {
        public const string FileName = "LanguageSettingsData";

        public Language Current = Language.Korean;
    }

    // 다른 설정값 주인들과 같은 모양. 값 하나(지금 언어)와 표를 들고 있다.
    public class LanguageSettings : SettingsHolder<LanguageSettingsData>, ILanguageSettings
    {
        [SerializeField]
        private TextTableData _table;

        public Language Current
        {
            get => Data.Current;
            set
            {
                if (Data.Current == value)
                {
                    return;
                }

                Data.Current = value;
                NotifyChanged();
            }
        }

        public string GetText(string key)
        {
            return _table == null ? key : _table.GetText(key, Current);
        }

        public TMP_FontAsset GetFont()
        {
            return _table == null ? null : _table.GetFont(Current);
        }
    }
}
