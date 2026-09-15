using System;
using TMPro;

namespace Refactoring
{
    // 지금 언어 창구. 설정창이 바꾸고, 화면의 글자들이 읽고 알림을 듣는다.
    public interface ILanguageSettings
    {
        event Action OnChanged;

        LanguageType Current { get; set; }

        string GetText(string key);
        TMP_FontAsset GetFont();
    }
}
