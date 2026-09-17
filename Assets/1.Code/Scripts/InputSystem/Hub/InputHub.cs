using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 액션 에셋의 입력을 수집해 현재 게임 모드의 처리기에게 전파한다.
    // 흐름: 액션 에셋에서 입력 수신 -> 현재 모드의 처리기 선택 -> OnPressed/OnMove 호출
    public class InputHub : MonoBehaviour
    {
        [Preserve, Inject(true)] private InputActionAsset _actionAsset;
        [Preserve, Inject] private List<IDomainInputHandler> _handlers;
        [Preserve, Inject] private IGameStateProvider _gameStateProvider;
        [Preserve, Inject(true)] private IInputKeySettings _keySettings;

        private readonly InputActionType[] _allActions = (InputActionType[])Enum.GetValues(typeof(InputActionType));

        // 컨텍스트(게임 모드)별 처리기 묶음. 입력은 현재 모드의 묶음에만 전달한다.
        private readonly Dictionary<GameStateType, IDomainInputHandler> _contextMap = new();
        // 구독한 액션과 그 종류. 콜백에서 어떤 입력인지 되찾고, 해제할 때 순회 대상이 된다.
        private readonly Dictionary<InputAction, InputActionType> _subscribed = new();

        private void OnEnable() => _actionAsset?.Enable();
        private void OnDisable() => _actionAsset?.Disable();
        
        
        private void Awake()
        {
            // 현재 모드를 모르면 입력을 어디로도 보낼 수 없다. 조용히 죽지 말고 즉시 멈춰 드러낸다.
            if (_gameStateProvider == null)
            {
                throw new InvalidOperationException($"{nameof(InputHub)}: {nameof(IGameStateProvider)} 주입 실패");
            }

            LoadChangedKeys();
            BuildContextMap();
            SubscribeActions();

            _gameStateProvider.OnChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (_gameStateProvider != null)
            {
                _gameStateProvider.OnChanged -= HandleStateChanged;
            }

            UnsubscribeActions();
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
                if (_contextMap.ContainsKey(handler.Context))
                {
                    // 한 모드에 처리기가 둘이면 나중 것은 영원히 입력을 못 받는다. 씬 배치 실수를 드러낸다.
                    Debug.LogWarning($"{nameof(InputHub)}: {handler.Context} 처리기가 둘 이상이다. {handler.GetType().Name}은(는) 무시된다");
                    continue;
                }

                _contextMap[handler.Context] = handler;
            }
        }

        private void SubscribeActions()
        {
            foreach (InputActionType actionType in _allActions)
            {
                InputAction action = _actionAsset?.FindAction(actionType.ToString());
                if (action == null)
                {
                    // enum에만 있고 액션 에셋에 없는 경우. 그 입력은 영원히 안 들어온다.
                    Debug.LogWarning($"{nameof(InputHub)}: 액션 에셋에 {actionType} 이(가) 없다");
                    continue;
                }

                _subscribed[action] = actionType;

                if (actionType == InputActionType.Movement)
                {
                    action.performed += HandleMovePerformed;
                    action.canceled += HandleMoveCanceled;
                }
                else
                {
                    action.performed += HandlePressPerformed;
                }
            }
        }

        // 액션 에셋은 씬이 바뀌어도 살아있는 SO다. 해제하지 않으면 씬마다 구독이 쌓여 입력이 중복 발동한다.
        private void UnsubscribeActions()
        {
            foreach (KeyValuePair<InputAction, InputActionType> pair in _subscribed)
            {
                if (pair.Value == InputActionType.Movement)
                {
                    pair.Key.performed -= HandleMovePerformed;
                    pair.Key.canceled -= HandleMoveCanceled;
                }
                else
                {
                    pair.Key.performed -= HandlePressPerformed;
                }
            }
            _subscribed.Clear();
        }

        private void HandleMovePerformed(InputAction.CallbackContext ctx) => RouteMove(ctx.ReadValue<Vector2>());

        private void HandleMoveCanceled(InputAction.CallbackContext ctx) => RouteMove(Vector2.zero);

        // 어떤 입력인지는 액션 자체로 되찾는다. 액션마다 람다를 만들면 -= 로 뗄 수 없어진다.
        private void HandlePressPerformed(InputAction.CallbackContext ctx)
        {
            if (_subscribed.TryGetValue(ctx.action, out InputActionType actionType))
            {
                RoutePressed(actionType);
            }
        }

        // 현재 모드를 받는 처리기가 없으면 입력을 버린다(컷씬처럼 아무도 입력을 받지 않는 모드가 정상 상황).
        private void RoutePressed(InputActionType action)
        {
            if (_contextMap.TryGetValue(_gameStateProvider.Current, out IDomainInputHandler handler))
            {
                handler.OnPressed(action);
            }
        }

        private void RouteMove(Vector2 move)
        {
            if (_contextMap.TryGetValue(_gameStateProvider.Current, out IDomainInputHandler handler))
            {
                handler.OnMove(move);
            }
        }
    }
}
