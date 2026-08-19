using UnityEngine;

namespace Refactoring
{
    // 켜고 끄고 싶은 씬 오브젝트에 붙이기. AttributeInjector가 씬을 스캔해 자동으로 모아준다.
    public class ToggleTarget : MonoBehaviour, IToggleTarget
    {
        [SerializeField] private ToggleTargetKey key;

        public ToggleTargetKey Key => key;
        public GameObject Target => gameObject;
    }
}
