using System;

namespace Refactoring
{
    // 마우스 감도 창구. 설정창은 여기에 쓰고, 카메라는 여기서 읽고 알림을 듣는다.
    public interface IMouseSettings
    {
        event Action OnChanged;

        float SpeedX { get; set; }
        float SpeedY { get; set; }
    }

    [Serializable]
    public class MouseSettingsData : ISaveData
    {
        public const string FileName = "MouseSettingsData";

        public float SpeedX = 1f;
        public float SpeedY = 1f;
    }

    public class MouseSettings : SettingsHolder<MouseSettingsData>, IMouseSettings
    {
        public float SpeedX
        {
            get => Data.SpeedX;
            set
            {
                Data.SpeedX = value;
                NotifyChanged();
            }
        }

        public float SpeedY
        {
            get => Data.SpeedY;
            set
            {
                Data.SpeedY = value;
                NotifyChanged();
            }
        }
    }
}
