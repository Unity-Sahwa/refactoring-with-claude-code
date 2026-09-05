using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 켜고 끌 씬 오브젝트 하나를 key와 함께 알린다. (AttributeInjector가 씬을 스캔해 모아준다)
    public class ToggleTarget : MonoBehaviour, IToggleTarget
    {
        [SerializeField] private ToggleTargetKey _key;

        public ToggleTargetKey Key => _key;
        public GameObject Target => gameObject;
    }
}
