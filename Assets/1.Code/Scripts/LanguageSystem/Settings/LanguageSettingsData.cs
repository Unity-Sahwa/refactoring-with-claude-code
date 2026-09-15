using System;

namespace Refactoring
{
    // 파일로 저장되는 언어 설정 한 덩어리.
    [Serializable]
    public class LanguageSettingsData : ISaveData
    {
        public const string FileName = "LanguageSettingsData";

        // public인 이유: 저장 파일에 직렬화되는 값이라 프로퍼티로 감싸면 저장에서 빠진다.
        public LanguageType Current = LanguageType.Korean;
    }
}
