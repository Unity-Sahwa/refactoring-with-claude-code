using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace Refactoring
{
    // ponytail: 조이스틱 드래그가 카메라를 돌리는 원인 추적용 임시 로거. 확인 끝나면 지울 것.
    public class InputDebugLogger : MonoBehaviour
    {
        [Preserve, Inject(true)] private InputActionAsset _actionAsset;

        private InputAction _look;

        private void Start() => _look = _actionAsset?.FindAction("Look");

        private void Update()
        {
            Vector2 look = _look?.ReadValue<Vector2>() ?? Vector2.zero;
            Vector2 l = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
            Vector2 r = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;

            if (look == Vector2.zero && l == Vector2.zero && r == Vector2.zero)
            {
                return;
            }

            Debug.Log($"[InputDebug] L={l} R={r} Look={look} pad={Gamepad.current?.name} src={_look?.activeControl?.path}");
        }
    }
}
