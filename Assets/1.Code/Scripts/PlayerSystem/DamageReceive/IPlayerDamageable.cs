namespace Refactoring
{
    // 씬에 유일한 플레이어 대상으로 데미지를 넘기고 싶을 때 이 인터페이스로 주입받는다.
    // IDamageable은 적/기믹 등도 구현하므로 DI가 하나로 못 좁혀서 따로 둔다.
    public interface IPlayerDamageable
    {
        void ApplyDamage(DamageInfo info);
    }
}
