using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 사용자 입력을 수집, 처리기들에게 전파
    // 입력이 들어오는 문은 두 개. 키보드/패드는 InputSystem 액션에서, 모바일 UI 버튼은 IInputSender로 들어옴.
    public class InputHub : MonoBehaviour, IInputSender
    {
        [Preserve, Inject(true)] private InputActionAsset _actionAsset;
        [Preserve, Inject] private List<IDomainInputHandler> _handlers;
        [Preserve, Inject(true)] private IGameStateProvider _gameStateProvider;
        [Preserve, Inject(true)] private IInputKeySettings _keySettings;

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

            if (_gameStateProvider != null)
            {
                _gameStateProvider.OnChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (_gameStateProvider != null)
            {
                _gameStateProvider.OnChanged -= HandleStateChanged;
            }
        }

        // 모드가 바뀌면 모든 처리기에 이동 정지를 알린다.
        // 안 그러면 키를 뗀 신호가 새 모드로 가버려서, 이전 모드는 마지막 이동값에 갇혀 계속 움직인다.
        private void HandleStateChanged(GameStateType state)
        {
            foreach (IDomainInputHandler handler in _handlers)
            {
                handler.OnMove(Vector2.zero);
            }
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

        // 현재 모드를 받는 처리기가 없으면 입력을 버린다(컷씬처럼 아무도 입력을 받지 않는 모드가 정상 상황).
        public void RoutePressed(InputActionType action)
        {
            if (_contextMap.TryGetValue(_gameStateProvider.Current, out IDomainInputHandler handler))
            {
                handler.OnPressed(action);
            }
        }

        public void RouteMove(Vector2 move)
        {
            if (_contextMap.TryGetValue(_gameStateProvider.Current, out IDomainInputHandler handler))
            {
                handler.OnMove(move);
            }
        }
    }
}
