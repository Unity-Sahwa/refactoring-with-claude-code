using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 컷씬 모드일 때 허브가 주는 입력 중 Interaction·Menu만 발행하고 나머지는 버린다.
    // 흐름: 허브가 OnPressed 호출 -> OnCutscenePressed로 발행(구독자: CutSceneSkipTrigger는 스킵, UIRoot는 메뉴)
    public class CutsceneInputHandler : MonoBehaviour, IDomainInputHandler, ICutsceneInputProvider
    {
        public event Action<InputActionType> OnCutscenePressed;

        public GameStateType Context => GameStateType.Cutscene;

        // 연출 중 이동·공격 입력은 무시한다. 통과시킨 입력을 어떻게 쓸지는 구독자가 정한다.
        public void OnPressed(InputActionType action)
        {
            if (action != InputActionType.Interaction && action != InputActionType.Menu)
            {
                return;
            }

            OnCutscenePressed?.Invoke(action);
        }

        public void OnMove(Vector2 move) { }
    }
}
