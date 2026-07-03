using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 메뉴 모드일 때 허브가 주는 입력을 UIRoot에 그대로 발행한다(배포만).
    // Context가 Menu라, 게임 모드가 Menu일 때만 허브에서 입력이 들어온다.
    public class MenuInputHandler : MonoBehaviour, IDomainInputHandler, IMenuInputProvider
    {
        public event Action<InputActionType> OnMenuPressed;

        public GameStateType Context => GameStateType.Menu;

        public void OnPressed(InputActionType action) => OnMenuPressed?.Invoke(action);

        // 대원_TODO: 메뉴에선 이동 입력을 쓰지 않는다. 다만 조작키 변경시 모든 키를 받는 부분은 생각해봐야할 듯하다
        public void OnMove(Vector2 move) { }
    }
}
