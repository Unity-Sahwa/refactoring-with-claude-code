using UnityEngine;

namespace Refactoring
{
    // 이펙트가 위치해야하는 곳(오브젝트)에 해당 클래스 붙이기
    public class EffectAttachPoint : MonoBehaviour, IEffectAttachPoint
    {
        [SerializeField] private EffectAttachPointType key;

        public EffectAttachPointType Key => key;
        public Transform Transform => transform;
    }
}
