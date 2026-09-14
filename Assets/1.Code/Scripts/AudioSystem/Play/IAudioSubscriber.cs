using System;

namespace Refactoring
{
    // 책임: 오디오 재생/정지 요청을 구독하는 계약.
    public interface IAudioSubscriber
    {
        IDisposable Register(Action<AudioPlayRequest> onPlay, Action<SoundType> onStop);
    }
}
