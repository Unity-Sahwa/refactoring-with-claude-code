// InputAction 콜백(PC·조이스틱)과 IMobileButton 이벤트(모바일 버튼)를 구독해
// OnInputPressed / OnInputReleased / OnMoveInput 이벤트를 발행.
// 폴링 없이 이벤트 기반으로 동작 — 입력 경로(키보드·UI 버튼)에 무관하게 액션 단위로 브로드캐스트.
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Refactoring
{
    public class InputEventBroadcaster : MonoBehaviour, IInjectRequester, IInjectTarget

    {
        [SerializeField] private InputActionAsset _actionAsset;

        public event Action<InputActionType> OnInputPressed;
        public event Action<InputActionType> OnInputReleased;
        public event Action<Vector2> OnMoveInput;

        public List<Type> TargetTypes => new List<Type> { typeof(IInputBlocker), typeof(IMobileButton) };

        public Type[] InterfaceTypes => new Type[] { typeof(InputEventBroadcaster) };


        private IInputBlocker _inputBlocker;

        private static readonly InputActionType[] AllActions = (InputActionType[])Enum.GetValues(typeof(InputActionType));

        private static InputEventBroadcaster _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private void OnEnable() => _actionAsset?.Enable();
        private void OnDisable() => _actionAsset?.Disable();

        public void Inject(Dictionary<Type, List<object>> targets)
        {
            if (targets.TryGetValue(typeof(IInputBlocker), out var blockers))
                _inputBlocker = blockers[0] as IInputBlocker;

            if (targets.TryGetValue(typeof(IMobileButton), out var buttons))
            {
                foreach (var obj in buttons)
                {
                    if (obj is IMobileButton btn)
                        SubscribeMobileButton(btn);
                }
            }

            SubscribeActions();
        }

        private void SubscribeActions()
        {
            foreach (InputActionType actionType in AllActions)
            {
                InputAction action = _actionAsset?.FindAction(actionType.ToString());
                if (action == null) continue;

                Action<InputAction.CallbackContext> performed;
                Action<InputAction.CallbackContext> canceled;

                if (actionType == InputActionType.Movement)
                {
                    performed = ctx => BroadcastMove(ctx.ReadValue<Vector2>());
                    canceled = _ => BroadcastMove(Vector2.zero);
                }
                else
                {
                    var captured = actionType;
                    performed = _ => HandlePressed(captured);
                    canceled = _ => HandleReleased(captured);
                }

                action.performed += performed;
                action.canceled += canceled;
            }
        }

        private void SubscribeMobileButton(IMobileButton btn)
        {
            Action down = () => HandlePressed(btn.ActionType);
            Action up = () => HandleReleased(btn.ActionType);
            btn.OnButtonDown += down;
            btn.OnButtonUp += up;
        }

        private void HandlePressed(InputActionType actionType)
        {
            if (_inputBlocker != null && _inputBlocker.IsInputBlocked) return;
            OnInputPressed?.Invoke(actionType);
        }

        private void HandleReleased(InputActionType actionType)
        {
            if (_inputBlocker != null && _inputBlocker.IsInputBlocked) return;
            OnInputReleased?.Invoke(actionType);
        }

        private void BroadcastMove(Vector2 value)
        {
            if (_inputBlocker != null && _inputBlocker.IsInputBlocked) return;
            OnMoveInput?.Invoke(value);
        }
    }
}
