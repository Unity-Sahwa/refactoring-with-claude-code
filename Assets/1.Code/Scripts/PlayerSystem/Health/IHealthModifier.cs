namespace Refactoring
{
    public interface IHealthModifier
    {
        // 체력을 깎고 적용 후 남은 체력을 돌려준다.
        float Decrease(float amount);
    }
}
