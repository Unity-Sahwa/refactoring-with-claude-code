// InputAction 콜백(PC·조이스틱)과 IMobileButton 이벤트(모바일 버튼)를 구독해
// OnInputPressed / OnInputReleased / OnMoveInput 이벤트를 발행.
// 폴링 없이 이벤트 기반으로 동작 — 입력 경로(키보드·UI 버튼)에 무관하게 액션 단위로 브로드캐스트.
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Refactoring
{
    public class InputEventBroadcaster : MonoBehaviour, IInterfaceInjectable, IInputEventProvider
    {
        public Dictionary<Type, List<object>> injectedImplements {get;} = new Dictionary<Type, List<object>>()
        {
            { typeof(IInputBlocker), new List<object>() },
            { typeof(IMobileButton), new List<object>() }
        };

        private static InputEventBroadcaster _instance;
        [SerializeField] private InputActionAsset _actionAsset;
        private IInputBlocker _inputBlocker;
        private InputActionType[] _allActions = (InputActionType[])Enum.GetValues(typeof(InputActionType));

        public event Action<InputActionType> OnInputPressed; //입력시 OnInputPressed?.Invoke(actionType)로 호출됨.
        public event Action<InputActionType> OnInputReleased; //입력 해제시 OnInputReleased?.Invoke(actionType)로 호출됨.
        public event Action<Vector2> OnMoveInput;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        void Start()
        {
            if(injectedImplements.TryGetValue(typeof(IMobileButton), out var buttons))
            {
                foreach (var obj in buttons)
                {
                    if (obj is IMobileButton btn)
                        SubscribeMobileButton(btn);
                }
            }
            SubscribeActions();
        }
        private void OnEnable() => _actionAsset?.Enable();
        private void OnDisable() => _actionAsset?.Disable();
        private void SubscribeActions()
        {
            foreach (InputActionType actionType in _allActions)
            {
                InputAction action = _actionAsset?.FindAction(actionType.ToString());
                if (action == null) continue;

                Action<InputAction.CallbackContext> performed;
                Action<InputAction.CallbackContext> canceled;

                //performed, canceled 이벤트에 추가 콜백 등록
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

                //InputSystem에 만들어둔 키에셋이 눌리면 performed, canceled 이벤트 실행됨.
                action.performed += performed;
                action.canceled += canceled;
            }
        }
        private void BroadcastMove(Vector2 value)
        {
            if (_inputBlocker != null && _inputBlocker.IsInputBlocked) return;
            OnMoveInput?.Invoke(value);
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
    }
}
