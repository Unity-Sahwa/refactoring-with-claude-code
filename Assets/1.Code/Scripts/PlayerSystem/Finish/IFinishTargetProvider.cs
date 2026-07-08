using System.Collections.Generic;

namespace Refactoring
{
    public interface IFinishTargetProvider
    {
        IReadOnlyList<Enemy> GatherStunTargets();       // 지금 범위 내 스턴 대상
        IReadOnlyList<Enemy> GatherExecuteTargets();    // 지금 범위 내 처형 대상
    }
}
