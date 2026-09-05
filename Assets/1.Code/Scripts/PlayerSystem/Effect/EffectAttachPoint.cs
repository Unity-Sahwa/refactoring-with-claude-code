using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 이펙트가 붙을 지점 하나를 key와 함께 알린다. (이펙트가 위치할 오브젝트에 이 클래스를 붙인다)
    public class EffectAttachPoint : MonoBehaviour, IEffectAttachPoint
    {
        [SerializeField] private EffectAttachPointType _key;

        public EffectAttachPointType Key => _key;
        public Transform Transform => transform;
    }
}
