using System;

namespace Refactoring
{
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
                default:
                    Data.Master = value;
                    break;
            }

            NotifyChanged();
        }
    }
}
