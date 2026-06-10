using UnityEngine;

namespace Refactoring
{
    // 입력 시스템 테스트 스크립트
    public class Test_InputSystem : MonoBehaviour
    {
        [SerializeField]
        private InputEventBroadcaster _broadcaster;

        private void OnEnable()
        {
            _broadcaster.OnInputPressed += HandleInputPressed;
            _broadcaster.OnMoveInput += HandleMoveInput;
        }

        private void OnDisable()
        {
            _broadcaster.OnInputPressed -= HandleInputPressed;
            _broadcaster.OnMoveInput -= HandleMoveInput;
        }

        private void HandleInputPressed(InputActionType action)
        {
            Debug.Log($"[Input] Pressed: {action}");
        }

        private void HandleMoveInput(Vector2 move)
        {
            if (move == Vector2.zero)
            {
                Debug.Log($"[Input] Move: {move}");
                return;
            }

            Debug.Log($"[Input] Move: {move}");
        }
    }
}
