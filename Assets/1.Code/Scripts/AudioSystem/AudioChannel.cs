using System;
using UnityEngine;

namespace Refactoring
{
    // 재생 요청을 위한 구조체. 옵션에 맞게 선택해서 호출하는 용도
    public struct AudioPlayRequest
    {
        public AudioId Id;
        public Vector3 Position;   // HasPosition이 true일 때만 의미 있음
        public bool HasPosition;   // 그 지점에서 한 번 재생(3D)
        public Transform Follow;   // 이동체에 붙어 따라가는 소리(창구가 자식으로 붙음)

        public static AudioPlayRequest Of(AudioId id)
        {
            return new AudioPlayRequest { Id = id };
        }
        public static AudioPlayRequest At(AudioId id, Vector3 position)
        {
            return new AudioPlayRequest { Id = id, Position = position, HasPosition = true };
        }
        public static AudioPlayRequest Following(AudioId id, Transform follow)
        {
            return new AudioPlayRequest { Id = id, Follow = follow };
        }
    }

    // 역할: 오디오 재생/정지 요청을 나르는 통로.
    [CreateAssetMenu(menuName = "EventChannel/AudioChannel")]
    public class AudioChannel : ScriptableObject
    {
        private Action<AudioPlayRequest> _onPlay;
        private Action<AudioId> _onStop;

        private void OnEnable()
        {
            _onPlay = null;
            _onStop = null;
        }

        public void RaisePlay(AudioPlayRequest request)
        {
            _onPlay?.Invoke(request);
        }
        public void RaiseStop(AudioId id) 
        {
            _onStop?.Invoke(id);
        }

        // 오디오를 재생/정지를 요청하는 이벤트 구독과 동시에 구독해제 가능한 IDisposable 반환
        public IDisposable Register(Action<AudioPlayRequest> onPlay, Action<AudioId> onStop)
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
