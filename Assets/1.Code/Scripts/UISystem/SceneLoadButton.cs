using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Refactoring
{
    // 클릭하면 연결된 Addressable 씬(맵)을 비동기로 불러온다. (로딩 화면은 추후 LoadingUI 재설계 때)
    // Single 모드라 이전 씬은 자동으로 내려감(메모리 해제 직접 관리 안 해도 됨).
    [RequireComponent(typeof(Button))]
    public class SceneLoadButton : MonoBehaviour
    {
        // 불러올 맵 씬을 Addressable 참조로 꽂는 칸. GUID 문자열이라 리임포트에 안 깨짐.
        [SerializeField]
        private AssetReference _mapScene;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            _mapScene.LoadSceneAsync(LoadSceneMode.Single);
        }
    }
}
