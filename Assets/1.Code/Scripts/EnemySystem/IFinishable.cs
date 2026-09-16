using UnityEngine;

namespace Refactoring
{
    // 책임: 플레이어 처형 대상으로서 스턴·처형을 받는 계약.
    public interface IFinishable : IEnemyDeadState
    {
        // 화면 안에 있는지 판정할 위치
        Vector3 Position { get; }

        void MotionStop(float waitTime);
        void Execution();
    }
}
