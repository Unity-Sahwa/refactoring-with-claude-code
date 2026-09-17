using System;

namespace Refactoring
{
    // 책임: 메뉴 모드 입력을 발행한다. IInputPressedProvider와 경로를 나눠 DI 충돌을 막는다.
    public interface IMenuInputProvider
    {
        event Action<InputActionType> OnMenuPressed;
    }
}
