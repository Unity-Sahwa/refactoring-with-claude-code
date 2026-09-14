using System;

namespace Refactoring
{
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
