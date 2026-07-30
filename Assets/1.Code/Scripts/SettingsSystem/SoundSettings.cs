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

        float GetVolume(VolumeType type);
        void SetVolume(VolumeType type, float value);
    }

    [Serializable]
    public class SoundSettingsData : ISaveData
    {
        public const string FileName = "SoundSettingsData";

        public float Master = 1f;
        public float Bgm = 1f;
        public float Sfx = 1f;
    }

    public class SoundSettings : SettingsHolder<SoundSettingsData>, ISoundSettings
    {
        public float GetVolume(VolumeType type)
        {
            switch (type)
            {
                case VolumeType.Bgm:
                    return Data.Bgm;
                case VolumeType.Sfx:
                    return Data.Sfx;
                default:
                    return Data.Master;
            }
        }

        public void SetVolume(VolumeType type, float value)
        {
            switch (type)
            {
                case VolumeType.Bgm:
                    Data.Bgm = value;
                    break;
                case VolumeType.Sfx:
                    Data.Sfx = value;
                    break;
                default:
                    Data.Master = value;
                    break;
            }

            NotifyChanged();
        }
    }
}
