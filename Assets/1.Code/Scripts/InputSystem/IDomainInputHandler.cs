using UnityEngine;

namespace Refactoring
{
    // 책임: 게임 모드별 입력 처리기의 공통 창구. 허브가 현재 모드의 처리기에만 입력을 넣는다.
    public interface IDomainInputHandler
    {
        // 이 처리기가 속한 게임 모드(컨텍스트). 허브는 현재 모드의 처리기에게만 입력을 보낸다.
        GameStateType Context { get; }

        void OnPressed(InputActionType action);
        void OnMove(Vector2 move);
    }
}
