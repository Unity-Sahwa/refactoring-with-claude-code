using System;
using UnityEngine;

namespace Refactoring
{
    public interface IInputEventProvider
    {
        public event Action<InputActionType> OnInputPressed;
        public event Action<InputActionType> OnInputReleased;
        public event Action<Vector2> OnMoveInput;
    } 
}

