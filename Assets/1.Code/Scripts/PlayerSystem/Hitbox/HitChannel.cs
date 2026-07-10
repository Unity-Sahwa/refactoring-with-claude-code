using System;
using UnityEngine;

namespace Refactoring
{
    // 타격 성공을 알리는 채널. Reporter가 발행하고, 소리·VFX·카메라 등이 각자 구독한다.
    // Reporter는 이 채널과 소리 id만 알 뿐 오디오 시스템(채널/플레이어)은 모른다.
    [CreateAssetMenu(menuName = "EventChannel/HitChannel")]
    public class HitChannel : ScriptableObject
    {
        private Action<AudioId, Vector3> _onHit;

        private void OnEnable() => _onHit = null;

        // sound: 이 타격에 낼 소리 id, point: 타격 지점
        public void Raise(AudioId sound, Vector3 point) => _onHit?.Invoke(sound, point);

        public IDisposable Register(Action<AudioId, Vector3> onHit)
        {
            _onHit += onHit;
            return new DisposeAction(() => _onHit -= onHit);
        }

        private class DisposeAction : IDisposable
        {
            private Action _dispose;
            public DisposeAction(Action dispose) => _dispose = dispose;
            public void Dispose() { _dispose?.Invoke(); _dispose = null; }
        }
    }
}
