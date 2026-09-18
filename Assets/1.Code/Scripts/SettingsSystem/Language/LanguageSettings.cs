using System;
using TMPro;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 다른 설정값 주인들과 같은 모양. 값 하나(지금 언어)와 표를 들고 있다.
    public class LanguageSettings : SettingsHolder<LanguageSettingsData>, ILanguageSettings
    {
        // 표는 DataContainer에 등록해두고 주입으로 받는다. 씬마다 손으로 꽂으면 빠뜨린 씬이 생긴다.
        [Preserve, Inject] private ITextTableData _table;

        private void Awake()
        {
            // 표가 없으면 번역·폰트 둘 다 못 준다. 조용히 죽지 말고 즉시 멈춰 드러낸다.
            if ((_table as UnityEngine.Object) == null)
            {
                throw new InvalidOperationException($"{nameof(LanguageSettings)}: {nameof(ITextTableData)} 주입 실패");
            }
        }

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

        public string GetText(string key) => _table.GetText(key, Current);

        public TMP_FontAsset GetFont() => _table.GetFont(Current);
    }
}
