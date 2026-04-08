// PC 플랫폼 입력을 읽는 IInputReader 구현체.
// InputBindingData에서 InputAction을 조회해 Unity Input System API로 판정.
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Refactoring
{
    public class PCInputReader : MonoBehaviour, IInputReader, IInjectTarget
    {
        [SerializeField]
        private InputBindingData _bindingData;

        public InputPlatformType Platform => InputPlatformType.PC;
        public Type[] InterfaceTypes => new[] { typeof(IInputReader) };

        // 매 프레임 항상 브로드캐스트 — Move 입력의 zero 벡터도 전달해야 이동이 정지됨
        public bool HasInput() => true;

        public bool IsPressed(InputActionType actionType)
        {
            return _bindingData.GetAction(actionType)?.WasPressedThisFrame() ?? false;
        }

        public bool IsHeld(InputActionType actionType)
        {
            return _bindingData.GetAction(actionType)?.IsPressed() ?? false;
        }

        public bool IsReleased(InputActionType actionType)
        {
            return _bindingData.GetAction(actionType)?.WasReleasedThisFrame() ?? false;
        }

        public Vector2 GetMoveInput()
        {
            return _bindingData.GetAction(InputActionType.Movement)?.ReadValue<Vector2>() ?? Vector2.zero;
        }
    }
}
