using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 오디오 재생/정지 요청을 나르는 통로. (실제 재생은 AudioPlayer 담당)
    [CreateAssetMenu(menuName = "Refactoring/Audio/AudioChannel")]
    public class AudioChannel : ScriptableObject
    {
        private Action<AudioPlayRequest> _onPlay;
        private Action<SoundType> _onStop;

        private void OnEnable()
        {
            _onPlay = null;
            _onStop = null;
        }

        public void RaisePlay(AudioPlayRequest request)
        {
            _onPlay?.Invoke(request);
        }

        public void RaiseStop(SoundType id)
        {
            _onStop?.Invoke(id);
        }

        // 구독 해제를 IDisposable로 돌려주는 이유: 구독한 쪽이 해제 함수를 따로 보관하지 않아도 된다.
        public IDisposable Register(Action<AudioPlayRequest> onPlay, Action<SoundType> onStop)
        {
            _onPlay += onPlay;
            _onStop += onStop;

            return new DisposeAction(() =>
            {
                _onPlay -= onPlay;
                _onStop -= onStop;
            });
        }

        private class DisposeAction : IDisposable
        {
            private Action _dispose;

            public DisposeAction(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                _dispose?.Invoke();
                _dispose = null;
            }
        }
    }
}
