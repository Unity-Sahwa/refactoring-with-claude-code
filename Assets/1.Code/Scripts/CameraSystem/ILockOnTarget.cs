using UnityEngine;

namespace Refactoring
{
    // 책임: 지금 락온으로 고정된 적이 누구인지 알려준다(없으면 null). (켜짐/꺼짐만 필요하면 ILockOnState)
    public interface ILockOnTarget
    {
        Collider LockedTarget { get; }
    }
}
