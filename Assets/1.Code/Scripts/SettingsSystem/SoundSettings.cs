using System;

namespace Refactoring
{
    // 소리 종류. 슬라이더 하나가 이 중 뭘 만지는지 고른다.
    public enum VolumeType
    {
        Master,
        Bgm,
        Sfx,
    }

    // 소리 크기 창구. 설정창은 여기에 쓰고, 오디오 쪽은 여기서 읽고 알림을 듣는다.
    public interface ISoundSettings
    {
        event Action OnChanged;

        float GetVolume(VolumeCategory type);
        void SetVolume(VolumeCategory type, float value);
    }

    [Serializable]
    public class SoundSettingsData : ISaveData
    {
        public const string FileName = "SoundSettingsData";

        public float Master = 1f;
        public float Bgm = 1f;
        public float Sfx = 1f;
        public float Enemy = 1f;
        public float Player = 1f;
        public float Environment = 1f;
        public float Ui = 1f;
    }

    public class SoundSettings : SettingsHolder<SoundSettingsData>, ISoundSettings
    {
        public float GetVolume(VolumeCategory type)
        {
            switch (type)
            {
                case VolumeCategory.Bgm:
                    return Data.Bgm;
                case VolumeCategory.Sfx:
                    return Data.Sfx;
                case VolumeCategory.Enemy:
                    return Data.Enemy;
                case VolumeCategory.Player:
                    return Data.Player;
                case VolumeCategory.Environment:
                    return Data.Environment;
                case VolumeCategory.Ui:
                    return Data.Ui;
                default:
                    return Data.Master;
            }
        }

        public void SetVolume(VolumeCategory type, float value)
        {
            switch (type)
            {
                case VolumeCategory.Bgm:
                    Data.Bgm = value;
                    break;
                case VolumeCategory.Sfx:
                    Data.Sfx = value;
                    break;
                case VolumeCategory.Enemy:
                    Data.Enemy = value;
                    break;
                case VolumeCategory.Player:
                    Data.Player = value;
                    break;
                case VolumeCategory.Environment:
                    Data.Environment = value;
                    break;
                case VolumeCategory.Ui:
                    Data.Ui = value;
                    break;
                default:
                    Data.Master = value;
                    break;
            }

            NotifyChanged();
        }
    }
}
