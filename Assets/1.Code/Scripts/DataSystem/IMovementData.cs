using System.Collections;
using UnityEngine;

namespace Refactoring
{
    public interface IMovementData
    {
        public float MoveSpeed {get;}
        public float RotateSpeed {get;}
    }
}