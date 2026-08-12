using System;

namespace Refactoring
{
<<<<<<< HEAD
=======
    // 소리 종류. 슬라이더 하나가 이 중 뭘 만지는지 고른다.
    public enum VolumeType
    {
        Master,
        Bgm,
        Sfx,
    }

>>>>>>> 13301c901c13016ba31dd9e76b9dbe839c667f42
    // 소리 크기 창구. 설정창은 여기에 쓰고, 오디오 쪽은 여기서 읽고 알림을 듣는다.
    public interface ISoundSettings
    {
        event Action OnChanged;

<<<<<<< HEAD
        float GetVolume(VolumeCategory type);
        void SetVolume(VolumeCategory type, float value);
=======
        float GetVolume(VolumeType type);
        void SetVolume(VolumeType type, float value);
>>>>>>> 13301c901c13016ba31dd9e76b9dbe839c667f42
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
<<<<<<< HEAD
        public float GetVolume(VolumeCategory type)
        {
            switch (type)
            {
                case VolumeCategory.Bgm:
                    return Data.Bgm;
                case VolumeCategory.Sfx:
=======
        public float GetVolume(VolumeType type)
        {
            switch (type)
            {
                case VolumeType.Bgm:
                    return Data.Bgm;
                case VolumeType.Sfx:
>>>>>>> 13301c901c13016ba31dd9e76b9dbe839c667f42
                    return Data.Sfx;
                default:
                    return Data.Master;
            }
        }

<<<<<<< HEAD
        public void SetVolume(VolumeCategory type, float value)
        {
            switch (type)
            {
                case VolumeCategory.Bgm:
                    Data.Bgm = value;
                    break;
                case VolumeCategory.Sfx:
=======
        public void SetVolume(VolumeType type, float value)
        {
            switch (type)
            {
                case VolumeType.Bgm:
                    Data.Bgm = value;
                    break;
                case VolumeType.Sfx:
>>>>>>> 13301c901c13016ba31dd9e76b9dbe839c667f42
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
