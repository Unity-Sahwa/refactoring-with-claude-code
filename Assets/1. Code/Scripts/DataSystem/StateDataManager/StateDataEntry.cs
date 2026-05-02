using System;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public struct StateDataEntry
    {
        public PlayerStateType stateType;
        public ScriptableObject data;
    }
}
