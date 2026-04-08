// 플랫폼별 원시 입력 읽기를 추상화하는 인터페이스.
// PCInputReader / MobileInputReader가 구현하며, InputEventBroadcaster가 DI로 주입받아 사용.
using UnityEngine;

namespace Refactoring
{
    public interface IInputReader
    {
        InputPlatformType Platform { get; }
        bool HasInput();
        bool IsPressed(InputActionType action);
        bool IsHeld(InputActionType action);
        bool IsReleased(InputActionType action);
        Vector2 GetMoveInput();
    }
}
