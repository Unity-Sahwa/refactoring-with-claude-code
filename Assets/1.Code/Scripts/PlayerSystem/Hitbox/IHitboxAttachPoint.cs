using UnityEngine;

namespace Refactoring
{
    public interface IHitboxAttachPoint
    {
        HitboxAttachPointType Key { get; }
        Transform Transform { get; }
    }
}
