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
                case VolumeCategory.BgmVolume:
                    return Data.Bgm;
                case VolumeCategory.SfxVolume:
                    return Data.Sfx;
                case VolumeCategory.EnemyVolume:
                    return Data.Enemy;
                case VolumeCategory.PlayerVolume:
                    return Data.Player;
                case VolumeCategory.EnvironmentVolume:
                    return Data.Environment;
                case VolumeCategory.UiVolume:
                    return Data.Ui;
                default:
                    return Data.Master;
            }
        }

        public void SetVolume(VolumeCategory type, float value)
        {
            switch (type)
            {
                case VolumeCategory.BgmVolume:
                    Data.Bgm = value;
                    break;
                case VolumeCategory.SfxVolume:
                    Data.Sfx = value;
                    break;
                case VolumeCategory.EnemyVolume:
                    Data.Enemy = value;
                    break;
                case VolumeCategory.PlayerVolume:
                    Data.Player = value;
                    break;
                case VolumeCategory.EnvironmentVolume:
                    Data.Environment = value;
                    break;
                case VolumeCategory.UiVolume:
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
