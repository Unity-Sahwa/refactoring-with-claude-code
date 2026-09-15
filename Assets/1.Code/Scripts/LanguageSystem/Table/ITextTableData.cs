using TMPro;

namespace Refactoring
{
    // 번역 표 창구. LanguageSettings가 이 인터페이스로만 표를 받는다.
    public interface ITextTableData
    {
        string GetText(string key, LanguageType language);
        TMP_FontAsset GetFont(LanguageType language);
    }
}
