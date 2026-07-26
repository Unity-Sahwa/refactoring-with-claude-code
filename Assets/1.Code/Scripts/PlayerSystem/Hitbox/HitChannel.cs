using System;
using UnityEngine;

namespace Refactoring
{
    // 역할: 플레이어 타격 성공시 미리 Register(...)했던 구독자들에게 Raise(...)를 통해 알린다.
    // 정보는 HitReport 묶음으로 실어보내고, 구독자는 그중 자기가 쓸 것만 꺼낸다.
    [CreateAssetMenu(menuName = "EventChannel/HitChannel")]
    public class HitChannel : ScriptableObject
    {
        private Action<HitReport> _onHit;

        private void OnEnable() => _onHit = null;

        public void Raise(HitReport report) => _onHit?.Invoke(report);

        public IDisposable Register(Action<HitReport> onHit)
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
