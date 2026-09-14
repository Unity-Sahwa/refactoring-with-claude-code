namespace Refactoring
{
    // 책임: 오디오 재생/정지를 요청하는 계약.
    public interface IAudioRaiser
    {
        void RaisePlay(AudioPlayRequest request);
        void RaiseStop(SoundType id);
    }
}
