// 대원TODO: ISoundSettings 분리. 다른 클래스에서 쓸껀데 왜 여기있는거지? 혹시 다른 곳에도 인터페이스가 클래스 안에 있으면 클래스 분리하기
// 대원TODO: 오디오 재생하는거랑, 오디오 셋팅하는 건 다름;; 셋팅(sound, volume 파트)은 다시 셋팅쪽으로 빠져야겟다
using System;

namespace Refactoring
{
    public interface ISoundSettings
    {
        event Action OnChanged;

        float GetVolume(VolumeCategory type);
        void SetVolume(VolumeCategory type, float value);
    }

}