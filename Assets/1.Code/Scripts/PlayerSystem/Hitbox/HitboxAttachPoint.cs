using UnityEngine;

namespace Refactoring
{
    // 히트박스가 위치해야하는 곳(오브젝트)에 해당 클래스 붙이기
    public class HitboxAttachPoint : MonoBehaviour, IHitboxAttachPoint
    {
        [SerializeField] private HitboxAttachPointType key;

        public HitboxAttachPointType Key => key;
        public Transform Transform => transform;
    }
}
