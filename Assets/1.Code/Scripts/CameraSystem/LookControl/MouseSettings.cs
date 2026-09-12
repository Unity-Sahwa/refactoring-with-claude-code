using System;

namespace Refactoring
{
    // 책임: 마우스 감도 창구. 설정창은 여기에 쓰고, 카메라는 여기서 읽고 알림을 듣는다.
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

        // public인 이유: 저장 파일에 직렬화되는 값이라 프로퍼티로 감싸면 저장에서 빠진다.
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
