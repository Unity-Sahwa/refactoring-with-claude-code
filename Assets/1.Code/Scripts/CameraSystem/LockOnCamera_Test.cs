using UnityEngine;

namespace Refactoring
{
    public class LockOnCamera_Test : MonoBehaviour, ILockOnState
    {
        [Inject] private IInputPressedProvider _inputProvider;
        public bool IsLockOn {get; private set;}

        private void Awake()
        {
            _inputProvider.OnInputPressed += OnPressed;
        }   
        private void OnDestroy()
        {
            if(_inputProvider != null)
            {
                _inputProvider.OnInputPressed -= OnPressed;
            }
        }

        private void OnPressed(InputActionType type)
        {
            if (type == InputActionType.LockOn) 
            {
                IsLockOn = !IsLockOn;
            }
        }
    }
}
