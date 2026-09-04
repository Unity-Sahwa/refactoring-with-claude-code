using System;

namespace Refactoring
{
    // 책임: 읽기용 — 현재 모드 조회 + 변경 알림. InputHub·입력 수신자가 구독한다.
    public interface IGameStateProvider
    {
        GameStateType Current { get; }
        event Action<GameStateType> OnChanged;
    }
}
