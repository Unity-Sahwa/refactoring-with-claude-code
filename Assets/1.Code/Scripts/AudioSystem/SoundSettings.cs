using System;
using System.Collections.Generic;

namespace Refactoring
{
    // 소리 크기 창구. 설정창은 여기에 쓰고, 오디오 쪽은 여기서 읽고 알림을 듣는다.
    // 대원TODO: ISoundSettings 분리. 다른 클래스에서 쓸껀데 왜 여기있는거지? 혹시 다른 곳에도 인터페이스가 클래스 안에 있으면 클래스 분리하기
    // 대원TODO: 오디오 재생하는거랑, 오디오 셋팅하는 건 다름;; 셋팅(sound, volume 파트)은 다시 셋팅쪽으로 빠져야겟다
    public interface ISoundSettings
    {
        event Action OnChanged;

        float GetVolume(VolumeCategory type);
        void SetVolume(VolumeCategory type, float value);
    }

    //대원TODO: 이건 한줄 적용 대상이 아닌가?
    //대원TODO: SoundSettingsData, VolumeEntry 이걸 굳이 분리해서 사용해야하는가
    [Serializable]
    public class SoundSettingsData : ISaveData
    {
        public const string FileName = "SoundSettingsData";

        // 볼륨 종류를 늘려도 여기는 그대로다. 이름을 열쇠로 쓰기 때문에 enum 순서가 바뀌어도 값이 밀리지 않는다.
        // 대신 enum 이름을 바꾸면 그 항목만 연결이 끊겨 기본값으로 돌아간다.
        public List<VolumeEntry> Volumes = new List<VolumeEntry>();
    }

    [Serializable]
    public class VolumeEntry
    {
        // public인 이유: 저장 파일에 직렬화되는 값이라 프로퍼티로 감싸면 저장에서 빠진다.
        public string Name;
        public float Value;
    }

    //대원TODO: 얘는 SettingsSystem을 쓰고 있는데? 심지어 SettingsHolder를 상속받고 있음. 설계 문제 신호 감지!!!
    public class SoundSettings : SettingsHolder<SoundSettingsData>, ISoundSettings
    {
        // 아직 손댄 적 없는 볼륨은 최대로 본다.
        private const float DefaultVolume = 1f;

        public float GetVolume(VolumeCategory type)
        {
            VolumeEntry entry = Find(type);

            return entry == null ? DefaultVolume : entry.Value;
        }

        public void SetVolume(VolumeCategory type, float value)
        {
            VolumeEntry entry = Find(type);

            if (entry == null)
            {
                entry = new VolumeEntry { Name = type.ToString() };
                Data.Volumes.Add(entry);
            }

            entry.Value = value;

            NotifyChanged();
        }

        private VolumeEntry Find(VolumeCategory type)
        {
            string name = type.ToString();

            foreach (VolumeEntry entry in Data.Volumes)
            {
                if (entry.Name == name)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
