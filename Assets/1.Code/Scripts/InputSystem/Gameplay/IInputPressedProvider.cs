using System;
using UnityEngine;

namespace Refactoring
{
    public interface IInputPressedProvider 
    {
        event Action<InputActionType> OnInputPressed; 
    }
}

