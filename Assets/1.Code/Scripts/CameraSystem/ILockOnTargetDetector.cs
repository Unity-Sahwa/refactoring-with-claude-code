using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 화면 조건을 통과한 적 후보를 매 프레임 내놓는다. (그중 누구를 고를지는 구독자 담당)
    public interface ILockOnTargetDetector
    {
        // 매 프레임 갱신되고, 없으면 빈 목록이다.
        IReadOnlyList<Collider> Candidates { get; }
    }
}
