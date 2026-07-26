using System;
using UnityEngine;

namespace Refactoring
{
    // 역할: UI 효과 알림 등록 및 해제/재생/정지 요청을 나르는 통로. 부르는 쪽과 실행하는 쪽은 서로 모르고 이 SO만 공유함.
    [CreateAssetMenu(menuName = "EventChannel/UIEffectChannel")]
    public class UIEffectChannel : ScriptableObject
    {
        private Action<UIEffectId> _onPlay;
        private Action<UIEffectId> _onStop;

        // 에디터 플레이모드 재시작/도메인 리로드 시 이전 구독이 남지 않게 비움.
        private void OnEnable()
        {
            _onPlay = null;
            _onStop = null;
        }

        public void RaisePlay(UIEffectId id) => _onPlay?.Invoke(id);
        public void RaiseStop(UIEffectId id) => _onStop?.Invoke(id);

        // 재생/정지 요청을 받기 위한 구독. 동시에 구독해제도 되게 IDisposable 반환.
        public IDisposable Register(Action<UIEffectId> onPlay, Action<UIEffectId> onStop)
        {
            _onPlay += onPlay;
            _onStop += onStop;

            return new DisposeAction(() =>
            {
                _onPlay -= onPlay;
                _onStop -= onStop;
            });
        }

        //대원_TODO: AudioChannel, HitChannel과 방식이 유사하기 때문에 공통 클래스로 뺄 수 있음.
        private class DisposeAction : IDisposable
        {
            private Action _dispose;
            public DisposeAction(Action dispose) => _dispose = dispose;

            public void Dispose()
            {
                _dispose?.Invoke();
                _dispose = null;
            }
        }
    }
}