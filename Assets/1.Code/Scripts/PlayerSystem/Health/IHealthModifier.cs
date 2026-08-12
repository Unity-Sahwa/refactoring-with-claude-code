namespace Refactoring
{
    public interface IHealthModifier
    {
        // 체력을 깎고 적용 후 남은 체력을 돌려준다.
        float Decrease(float amount);

        // 현재 체력을 그 값으로 맞춘다. 저장 슬롯을 불러올 때 쓴다.
        void SetCurrent(float value);
    }
}
