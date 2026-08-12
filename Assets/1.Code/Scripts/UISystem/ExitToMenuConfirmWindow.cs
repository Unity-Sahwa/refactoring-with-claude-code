using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Refactoring
{
    // "메뉴로 나갈까요?" 창. 정지메뉴에서 쓴다.
    public class ExitToMenuConfirmWindow : ConfirmWindow
    {
        [SerializeField]
        private AssetReference _mainMenuScene;

        protected override void RunYes()
        {
            _mainMenuScene.LoadSceneAsync(LoadSceneMode.Single);
        }
    }
}
