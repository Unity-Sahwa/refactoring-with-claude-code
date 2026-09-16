namespace Refactoring
{
    // 책임: 적이 죽었는지 읽기 전용으로 알린다. (isDead를 훔쳐보는 여러 시스템 공용)
    public interface IEnemyDeadState
    {
        bool IsDead { get; }
    }
}
