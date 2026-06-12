namespace Refactoring
{
    // 피해를 받을 수 있는 대상이 구현하는 계약.
    // 적, 부술 수 있는 오브젝트 등 종류와 무관하게 이 인터페이스만 구현하면 히트박스가 정보를 전달한다.
    // 피해 적용·죽음·넉백을 어떻게 처리할지는 구현하는 쪽이 정한다.
    public interface IDamageable
    {
        void ApplyDamage(DamageInfo info);
    }
}
