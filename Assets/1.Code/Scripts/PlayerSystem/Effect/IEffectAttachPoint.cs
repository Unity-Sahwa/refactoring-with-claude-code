using UnityEngine;

namespace Refactoring
{
    // 책임: 이펙트가 붙을 지점 하나를 key와 Transform으로 알린다.
    public interface IEffectAttachPoint
    {
        EffectAttachPointType Key { get; }
        Transform Transform { get; }
    }
}
