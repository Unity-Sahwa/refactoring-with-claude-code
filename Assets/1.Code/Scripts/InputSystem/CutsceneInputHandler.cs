using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 컷씬 모드일 때 허브가 주는 입력 중 메뉴 열기만 발행하고 나머지는 버린다.
    // 왜 필요: 이 처리기가 없으면 컷씬 중 모든 입력이 버려져 메뉴조차 열 수 없다.
    public class CutsceneInputHandler : MonoBehaviour, IDomainInputHandler, ICutsceneInputProvider
    {
        public event Action<InputActionType> OnCutscenePressed;

        public GameStateType Context => GameStateType.Cutscene;

        // 연출 중 이동·공격 입력은 무시하고 메뉴만 통과시킨다.
        public void OnPressed(InputActionType action)
        {
            if (action != InputActionType.Menu)
            {
                return;
            }

            OnCutscenePressed?.Invoke(action);
        }

        public void OnMove(Vector2 move) { }
    }
}
