using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Refactoring
{
    // 컷만화 창. 창 자체가 버튼이라 아무 데나 누르면 다음 컷으로 넘어간다.
    // 컷 하나하나는 창이 아니라서 스택에 안 들어간다(ESC 한 번에 통째로 닫힘).
    [RequireComponent(typeof(Button))]
    public class CutToonWindow : UIWindow
    {
        // 순서대로 보여줄 컷 오브젝트들.
        [SerializeField]
        private GameObject[] _cuts;

        // 다 넘겼을 때 시작할 게임 씬.
        [SerializeField]
        private AssetReference _nextScene;

        private int _currentIndex;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void OnEnable()
        {
            _currentIndex = 0;
            ShowCurrentCut();
        }

        private void HandleClicked()
        {
            _currentIndex++;

            if (_currentIndex >= _cuts.Length)
            {
                _nextScene.LoadSceneAsync(LoadSceneMode.Single);
                return;
            }

            ShowCurrentCut();
        }

        private void ShowCurrentCut()
        {
            for (int i = 0; i < _cuts.Length; i++)
            {
                _cuts[i].SetActive(i == _currentIndex);
            }
        }
    }
}
