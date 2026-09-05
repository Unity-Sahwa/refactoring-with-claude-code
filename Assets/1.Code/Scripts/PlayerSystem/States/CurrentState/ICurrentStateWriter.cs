namespace Refactoring
{
    // 책임: 현재 상태를 기록한다(쓰기 전용).
    public interface ICurrentStateWriter
    {
        void SetCurrentState(PlayerStateType state);
    }
}
