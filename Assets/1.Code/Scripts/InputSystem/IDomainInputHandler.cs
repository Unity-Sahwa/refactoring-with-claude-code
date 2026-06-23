using UnityEngine;

namespace Refactoring
{
    // 분야별 입력 처리기 공통 인터페이스. 허브가 모든 처리기에 입력을 전달한다.
    // 받은 입력을 자기 도메인 행동으로 번역하고, 도메인 내부 제한(스킬 중 막기 등)도 여기서 처리한다.
    public interface IDomainInputHandler
    {
        // 이 처리기가 속한 게임 모드(컨텍스트). 허브는 현재 모드의 처리기에게만 입력을 보낸다.
        GameStateType Context { get; }

        void OnPressed(InputActionType action);
        void OnMove(Vector2 move);
    }
}
