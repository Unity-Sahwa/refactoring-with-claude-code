// 런타임 키 리바인딩을 처리하는 서비스.
// StartRebinding() 호출 시 Unity Input System의 PerformInteractiveRebinding()으로 비동기 감지.
// 완료 후 IInputSaveHandler를 통해 전체 override JSON을 저장.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Refactoring
{
    public class InputRebindingService : MonoBehaviour, IInjectTarget, IInjectRequester
    {
        [SerializeField]
        private InputBindingData _bindingData;

        public event Action<InputPlatformType> OnBindingChanged;

        public Type[] InterfaceTypes => new[] { typeof(InputRebindingService) };
        public List<Type> TargetTypes => new List<Type> { typeof(IInputSaveHandler) };

        private IInputSaveHandler _saveHandler;
        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;

        private void OnDestroy()
        {
            _rebindOperation?.Dispose();
        }

        public void Inject(Dictionary<Type, List<object>> targets)
        {
            if (targets.TryGetValue(typeof(IInputSaveHandler), out var handlers) && handlers.Count > 0)
            {
                _saveHandler = handlers[0] as IInputSaveHandler;
            }

            // 주입 직후 저장된 바인딩 복원
            _bindingData?.ApplyOverrideJson(_saveHandler?.LoadBindings());
        }

        public void StartRebinding(InputActionType actionType, InputPlatformType platform)
        {
            InputAction action = _bindingData?.GetAction(actionType);

            if (action == null)
            {
                return;
            }

            action.Disable();

            _rebindOperation = action.PerformInteractiveRebinding()
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .OnComplete(_ => HandleRebindComplete(action, platform))
                .OnCancel(_ => HandleRebindCancelled(action))
                .Start();
        }

        public void CancelRebinding()
        {
            _rebindOperation?.Cancel();
        }

        private void HandleRebindComplete(InputAction action, InputPlatformType platform)
        {
            action.Enable();
            _rebindOperation?.Dispose();
            _rebindOperation = null;

            OnBindingChanged?.Invoke(platform);
            _saveHandler?.SaveBindings(_bindingData.SaveOverrideJson());
        }

        private void HandleRebindCancelled(InputAction action)
        {
            action.Enable();
            _rebindOperation?.Dispose();
            _rebindOperation = null;
        }
    }
}
