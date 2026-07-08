using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 화면 조건(카메라 안 + 안 가려짐 + 플레이어 기준 화면 안쪽)을 통과한 적 후보들을 매프레임 내놓는다.
    // 자신은 "거르기"만 한다. 그중 누구를 락온/처형 대상으로 고를지는 구독자의 일.
    public interface ILockOnTargetDetector
    {
        IReadOnlyList<Collider> Candidates { get; }   // 매프레임 갱신, 없으면 빈 목록
    }
}
