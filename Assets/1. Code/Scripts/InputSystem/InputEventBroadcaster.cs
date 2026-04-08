// IInputReader를 매 프레임 폴링해 OnInputPressed / OnInputReleased / OnMoveInput 이벤트를 발행.
// DI로 Reader·Blocker를 주입받으며, SetPlatform()으로 활성 플랫폼 전환 가능.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class InputEventBroadcaster : MonoBehaviour, IInjectRequester
    {
        public event Action<InputActionType> OnInputPressed;
        public event Action<InputActionType> OnInputReleased;
        public event Action<Vector2> OnMoveInput;

        public List<Type> TargetTypes => new List<Type> { typeof(IInputReader), typeof(IInputBlocker) };

        private static readonly HashSet<InputActionType> MoveActions = new HashSet<InputActionType>
        {
            InputActionType.Movement
        };

        private static readonly InputActionType[] AllActions =
            (InputActionType[])Enum.GetValues(typeof(InputActionType));

        private List<IInputReader> _readers = new();
        private IInputBlocker _inputBlocker;
        private IInputReader _activeReader;

        public void Inject(Dictionary<Type, List<object>> targets)
        {
            if (targets.TryGetValue(typeof(IInputReader), out var readers))
            {
                foreach (var obj in readers)
                {
                    if (obj is IInputReader reader)
                    {
                        _readers.Add(reader);
                    }
                }
            }

            if (targets.TryGetValue(typeof(IInputBlocker), out var blockers) && blockers.Count > 0)
            {
                _inputBlocker = blockers[0] as IInputBlocker;
            }

            SetPlatform(InputPlatformType.PC);
        }

        public void SetPlatform(InputPlatformType platform)
        {
            _activeReader = _readers.Find(r => r.Platform == platform);
        }

        private void Update()
        {
            if (_activeReader == null) return;
            if (_inputBlocker != null && _inputBlocker.IsInputBlocked) return;
            if (!_activeReader.HasInput()) return;

            BroadcastMoveInput();
            BroadcastActionInput();
        }

        private void BroadcastMoveInput()
        {
            Vector2 moveInput = _activeReader.GetMoveInput();
            OnMoveInput?.Invoke(moveInput);
        }

        private void BroadcastActionInput()
        {
            foreach (InputActionType action in AllActions)
            {
                if (MoveActions.Contains(action))
                {
                    continue;
                }

                if (_activeReader.IsPressed(action))
                {
                    OnInputPressed?.Invoke(action);
                }

                if (_activeReader.IsReleased(action))
                {
                    OnInputReleased?.Invoke(action);
                }
            }
        }
    }
}
