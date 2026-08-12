using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 게임을 끄는 버튼. 종료 안내창의 "예" 버튼에 붙인다.
    [RequireComponent(typeof(Button))]
    public class QuitGameButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
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
