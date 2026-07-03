using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Refactoring
{
    // 클릭하면 지정한 빌드 인덱스의 씬을 비동기로 불러온다. (로딩 화면은 추후 LoadingUI 재설계 때)
    [RequireComponent(typeof(Button))]
    public class SceneLoadButton : MonoBehaviour
    {
        // Build Settings의 씬 순번. 값이라 리임포트에 안 깨짐.
        [SerializeField]
        private int _sceneBuildIndex;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            SceneManager.LoadSceneAsync(_sceneBuildIndex);
        }
    }
}
