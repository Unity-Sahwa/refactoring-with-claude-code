using System;
using UnityEngine;

namespace Refactoring
{
    // 역할: 현재 상태를 담아 두는 공유 SO. 쓰는 쪽과 읽는 쪽이 서로를 모르고 이 SO만 공유한다.
    [CreateAssetMenu(menuName = "EventChannel/PlayerCurrentStateChannel")]
    public class PlayerCurrentStateChannel : ScriptableObject, ICurrentStateWriter, ICurrentStateProvider
    {
        public PlayerStateType CurrentState { get; private set; }

        public event Action<PlayerStateType> StateChanged;

        // 에디터 플레이모드 재시작/도메인 리로드 시 이전 값이 남지 않게 초기화한다.
        private void OnEnable() => CurrentState = default;

        public void SetCurrentState(PlayerStateType state)
        {
            CurrentState = state;
            StateChanged?.Invoke(state);
        }
    }
}
