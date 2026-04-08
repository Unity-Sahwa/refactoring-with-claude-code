using UnityEngine;

namespace Refactoring
{
    // 입력 시스템 테스트 스크립트
    // PC:     W/S/A/D 이동, Q = Skill, E = Skill2
    // Mobile: Inspector에서 MobileReader 연결 후 UI 버튼 EventTrigger에 메서드 연결
    public class Test_InputSystem : MonoBehaviour
    {
        [SerializeField]
        private InputEventBroadcaster _broadcaster;

        [Tooltip("모바일 버튼 테스트 시 연결")]
        [SerializeField]
        private MobileInputReader _mobileReader;

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
                return;
            }

            Debug.Log($"[Input] Move: {move}");
        }
    }
}
