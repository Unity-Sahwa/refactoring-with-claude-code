using System;

namespace Refactoring
{
    public enum GameStateRole
    {
        Requester,
        Manager
    }

    public enum GameStateType
    {
        InputBlockedByMenu,
        InputUnblockedByMenu,
        InputBlockedBySequence,
        InputUnblockedBySequence,
        Paused,
        Resumed,
        Restarted,
        GameOver
    }

    public interface IGameStateEvent
    {
        GameStateRole Role { get; }
        event Action<GameStateType> OnStateChanged;
    }
}
