using UnityEngine;
using UnityEngine.InputSystem;

namespace Refactoring
{

public class InputSystemTester : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset _actions;

    private InputAction _movement;
    private InputAction _normalAttack;
    private InputAction _specialAttack;
    private InputAction _finishAttack;
    private InputAction _dash;
    private InputAction _lockOn;
    private InputAction _menu;

    private void Awake()
    {
        _movement     = _actions["Movement"];
        _normalAttack = _actions["NormalAttack"];
        _specialAttack = _actions["SpecialAttack"];
        _finishAttack = _actions["FinishAttack"];
        _dash         = _actions["Dash"];
        _lockOn       = _actions["LockOn"];
        _menu         = _actions["Menu"];
    }

    private void OnEnable()
    {
        _actions.Enable();

        _movement.performed += HandleMovementPerformed;
        _movement.canceled  += HandleMovementCanceled;

        _normalAttack.performed  += HandleNormalAttack;
        _specialAttack.performed += HandleSpecialAttack;
        _finishAttack.performed  += HandleFinishAttack;
        _dash.performed          += HandleDash;
        _lockOn.performed        += HandleLockOn;
        _menu.performed          += HandleMenu;
    }

    private void OnDisable()
    {
        _movement.performed -= HandleMovementPerformed;
        _movement.canceled  -= HandleMovementCanceled;

        _normalAttack.performed  -= HandleNormalAttack;
        _specialAttack.performed -= HandleSpecialAttack;
        _finishAttack.performed  -= HandleFinishAttack;
        _dash.performed          -= HandleDash;
        _lockOn.performed        -= HandleLockOn;
        _menu.performed          -= HandleMenu;

        _actions.Disable();
    }

    private void HandleMovementPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[Input] Move: {ctx.ReadValue<Vector2>()} / device: {ctx.control.device.name}");
    }

    private void HandleMovementCanceled(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[Input] Move: 정지 (0, 0) / device: {ctx.control.device.name}");
    }

    private void HandleNormalAttack(InputAction.CallbackContext ctx)  => Debug.Log($"[Input] NormalAttack / device: {ctx.control.device.name}");
    private void HandleSpecialAttack(InputAction.CallbackContext ctx) => Debug.Log($"[Input] SpecialAttack / device: {ctx.control.device.name}");
    private void HandleFinishAttack(InputAction.CallbackContext ctx)  => Debug.Log($"[Input] FinishAttack / device: {ctx.control.device.name}");
    private void HandleDash(InputAction.CallbackContext ctx)          => Debug.Log($"[Input] Dash / device: {ctx.control.device.name}");
    private void HandleLockOn(InputAction.CallbackContext ctx)        => Debug.Log($"[Input] LockOn / device: {ctx.control.device.name}");
    private void HandleMenu(InputAction.CallbackContext ctx)          => Debug.Log($"[Input] Menu / device: {ctx.control.device.name}");
}

}
