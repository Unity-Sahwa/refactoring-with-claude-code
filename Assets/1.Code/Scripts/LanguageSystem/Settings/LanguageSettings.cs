using TMPro;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 다른 설정값 주인들과 같은 모양. 값 하나(지금 언어)와 표를 들고 있다.
    public class LanguageSettings : SettingsHolder<LanguageSettingsData>, ILanguageSettings
    {
        // 표는 DataContainer에 등록해두고 주입으로 받는다. 씬마다 손으로 꽂으면 빠뜨린 씬이 생긴다.
        [Preserve, Inject] private TextTableData _table;

        public LanguageType Current
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
