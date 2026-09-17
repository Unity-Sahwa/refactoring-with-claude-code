using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;


namespace Refactoring
{
    // 책임: 모바일 화면 버튼을 InputActionAsset의 해당 액션 컨트롤과 연결한다.
    // 흐름: 시작 시 현재 바인딩 경로로 맞춤 -> 설정에서 리바인딩되면 InputSystem.onActionChange로 감지해 다시 맞춤
    [RequireComponent(typeof(OnScreenButton))]
    public class MobileInputButton : MonoBehaviour
    {
        [SerializeField] private InputActionType _actionType;
        [Preserve, Inject(true)] private InputActionAsset _actionAsset;

        private OnScreenButton _onScreenButton;

        private void Awake()
        {
            _onScreenButton = GetComponent<OnScreenButton>();
        }

        private void OnEnable()
        {
            InputSystem.onActionChange += HandleActionChange;
        }

        private void OnDisable()
        {
            InputSystem.onActionChange -= HandleActionChange;
        }

        // OnScreenButton은 자기 디바이스를 자신의 OnEnable에서 만든다. 이 컴포넌트가 같은 프레임 OnEnable에서
        // 경로를 건드리면 아직 없는 컨트롤을 덮어써 에러가 난다. Start는 모든 OnEnable이 끝난 뒤 실행되므로 여기서 맞춘다.
        private void Start()
        {
            SyncPath();
        }

        private void HandleActionChange(object subject, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged || subject is not InputAction action)
            {
                return;
            }

            if (action.name == _actionType.ToString())
            {
                SyncPath();
            }
        }

        private void SyncPath()
        {
            InputAction action = _actionAsset?.FindAction(_actionType.ToString());
            if (action == null || action.bindings.Count == 0)
            {
                return;
            }

            _onScreenButton.controlPath = action.bindings[0].effectivePath;
        }
    }
}
