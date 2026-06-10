using UnityEngine;

namespace Refactoring
{
    public interface IEffectAttachPoint
    {
        EffectAttachPointType Key { get; }
        Transform Transform { get; }
    }
}
