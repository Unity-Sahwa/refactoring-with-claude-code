using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 메뉴 모드일 때 허브가 주는 입력을 UIRoot에 그대로 발행한다(배포만).
    public class MenuInputHandler : MonoBehaviour, IDomainInputHandler, IMenuInputProvider
    {
        public event Action<InputActionType> OnMenuPressed;

        public GameStateType Context => GameStateType.Menu;

        public void OnPressed(InputActionType action) => OnMenuPressed?.Invoke(action);

        public void OnMove(Vector2 move) { }
    }
}
