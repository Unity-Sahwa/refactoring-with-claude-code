using UnityEngine;

namespace Refactoring
{
    // "게임을 끌까요?" 창.
    public class QuitConfirmWindow : ConfirmWindow
    {
        protected override void RunYes()
        {
#if UNITY_EDITOR
            // 에디터에선 Application.Quit이 안 먹혀서 플레이만 멈춘다.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
