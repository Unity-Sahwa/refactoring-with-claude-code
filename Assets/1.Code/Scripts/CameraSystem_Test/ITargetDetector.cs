using System;
using UnityEngine;

namespace Refactoring
{
    // 지금 조준할 만한 적 하나를 찾아 알려준다. 락온·마커·외곽선이 이걸 구독한다.
    // 자신은 적을 "찾기"만 한다. 마커 표시·머리위치·HUD 같은 건 구독자의 일.
    public interface ITargetDetector
    {
        Collider CurrentTarget { get; }           // 지금 조준 가능한 적(없으면 null)
        event Action<Collider> OnTargetChanged;   // 대상이 바뀔 때만 알림(없어지면 null로 알림)
    }
}
