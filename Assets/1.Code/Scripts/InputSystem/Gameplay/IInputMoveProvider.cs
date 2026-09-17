using System;
using UnityEngine;

namespace Refactoring
{
    public interface IInputMoveProvider    
    {
        event Action<Vector2> OnVector2Input; 
    }
}

