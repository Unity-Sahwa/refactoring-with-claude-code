using UnityEngine;

namespace Refactoring
{
    public interface IToggleTarget
    {
        ToggleTargetKey Key { get; }
        GameObject Target { get; }
    }
}
