namespace Refactoring
{
    // 피해를 받을 수 있는 대상이 구현하는 계약. 피해 적용·죽음·넉백 처리는 구현하는 쪽이 정한다.
    public interface IDamageable
    {
        void ApplyDamage(DamageInfo info);
    }
}
