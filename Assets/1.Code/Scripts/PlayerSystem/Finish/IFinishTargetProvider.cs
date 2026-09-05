using System.Collections.Generic;

namespace Refactoring
{
    // 책임: 요청 시점의 스턴·처형 대상 목록을 제공하는 계약.
    public interface IFinishTargetProvider
    {
        // 지금 범위 내 스턴 대상
        IReadOnlyList<Enemy> GatherStunTargets();

        // 지금 범위 내 처형 대상
        IReadOnlyList<Enemy> GatherExecuteTargets();
    }
}
