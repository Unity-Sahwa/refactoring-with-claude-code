using System;

namespace Refactoring
{
    // PC/모바일 창구. 설정창이 고르고, 모바일 UI와 튜토리얼 가이드가 읽는다.
    public interface IPlatformSettings
    {
        event Action OnChanged;

        bool IsMobile { get; set; }
    }

    [Serializable]
    public class PlatformSettingsData : ISaveData
    {
        public const string FileName = "PlatformSettingsData";

        public bool IsMobile;
    }

    public class PlatformSettings : SettingsHolder<PlatformSettingsData>, IPlatformSettings
    {
        public bool IsMobile
        {
            get => Data.IsMobile;
            set
            {
                Data.IsMobile = value;
                NotifyChanged();
            }
        }
    }
}
