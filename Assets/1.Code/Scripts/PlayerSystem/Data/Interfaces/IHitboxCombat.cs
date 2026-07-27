namespace Refactoring
{
    // 히트박스의 전투값 계약. 감지 컴포넌트가 이 값만 읽어 DamageInfo를 만든다.
    // 켜고 끄기 정보(IPlayerHitbox)와 분리해, 같은 데이터라도 관심사가 섞이지 않게 한다.
    public interface IHitboxCombat
    {
        float Damage { get; }
        InkColor Color { get; }
        float InkStack { get; }
    }
}
