namespace Refactoring
{
    // 역할: 현재 활성 캐릭터가 어느 상태인지 "조회"만 제공한다(읽기 전용).
    public interface ICurrentStateProvider
    {
        PlayerStateType CurrentState { get;}
    }
}
