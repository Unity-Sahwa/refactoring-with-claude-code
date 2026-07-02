using UnityEngine;

namespace Refactoring
{
    // 지금 락온으로 "고정된" 적이 누구인지 알려준다(없으면 null).
    // 카메라가 이 대상을 비추거나 TargetGroup에 넣을 때 구독한다.
    // IsLockOn(켜짐/꺼짐)만 필요한 애니·입력과 달리, 이건 "대상"이 필요한 쪽(카메라)을 위한 별도 통로.
    public interface ILockOnTarget
    {
        Collider LockedTarget { get; }
    }
}
