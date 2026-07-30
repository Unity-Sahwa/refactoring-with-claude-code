using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Refactoring
{
    // 책임: InputSystem에서 입력 수집, 처리기들에게 전파
    public class InputHub : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actionAsset;
        [Inject] private List<IDomainInputHandler> _handlers;
        [Inject(true)] private IGameStateProvider _gameStateProvider;
        [Inject(true)] private IInputKeySettings _keySettings;

        private readonly InputActionType[] _allActions = (InputActionType[])Enum.GetValues(typeof(InputActionType));

        // 컨텍스트(게임 모드)별 처리기 묶음. 입력은 현재 모드의 묶음에만 전달한다.
        private readonly Dictionary<GameStateType, IDomainInputHandler> _contextMap = new();
        private static readonly List<IDomainInputHandler> _empty = new();

        private void OnEnable() => _actionAsset?.Enable();
        private void OnDisable() => _actionAsset?.Disable();
        
        
        private void Awake()
        {
            LoadChangedKeys();
            BuildContextMap();
            SubscribeActions();
        }

        // 설정에서 바꿔둔 조작키를 액션 에셋에 덮어씌운다. 바꾼 적 없으면 그냥 기본값.
        private void LoadChangedKeys()
        {
            string bindings = _keySettings?.Bindings;

            if (string.IsNullOrEmpty(bindings))
            {
                return;
            }

            _actionAsset?.LoadBindingOverridesFromJson(bindings);
        }

        private void BuildContextMap()
        {
            foreach (IDomainInputHandler handler in _handlers)
            {
                if (!_contextMap.ContainsKey(handler.Context))
                {
                    _contextMap[handler.Context] = handler;
                }
            }
        }

        private void SubscribeActions()
        {
            foreach (InputActionType actionType in _allActions)
            {
                InputAction action = _actionAsset?.FindAction(actionType.ToString());
                if (action == null) continue;

                if (actionType == InputActionType.Movement)
                {
                    action.performed += ctx => RouteMove(ctx.ReadValue<Vector2>());
                    action.canceled += _ => RouteMove(Vector2.zero);
                }
                else
                {
                    InputActionType captured = actionType;
                    action.performed += _ => RoutePressed(captured);
                }
            }
        }

        private void RoutePressed(InputActionType action)
        {
            _contextMap[_gameStateProvider.Current].OnPressed(action);
        }

        private void RouteMove(Vector2 move)
        {
            _contextMap[_gameStateProvider.Current].OnMove(move);
        }
    }
}
