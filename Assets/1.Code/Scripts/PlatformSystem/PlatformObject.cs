using UnityEngine;

namespace Refactoring
{
    // 플랫폼에 따라 켜고 꺼질 오브젝트에 붙이는 표식. PlatformController가 모아서 처리한다.
    public class PlatformObject : MonoBehaviour
    {
        // 체크하면 모바일에서만 켜지고, 해제하면 PC에서만 켜진다.
        [SerializeField] private bool _isMobileOnly = true;
        public bool IsMobileOnly => _isMobileOnly;
    }
}