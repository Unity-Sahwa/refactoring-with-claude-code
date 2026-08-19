using System;

namespace Refactoring
{
    // 역할: 현재 활성 캐릭터가 어느 상태인지 "조회"만 제공한다(읽기 전용).
    public interface ICurrentStateProvider
    {
        PlayerStateType CurrentState { get;}

        // 상태 전환이 실제로 일어날 때(같은 상태 재진입 제외) 새 상태를 실어 알린다.
        event Action<PlayerStateType> StateChanged;
    }
}
