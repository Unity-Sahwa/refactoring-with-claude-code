namespace Refactoring
{
    // 책임: 덧칠 스택이 최대치인지, 그 최대치가 얼만지 읽기 전용으로 알린다.
    public interface IPaintOverState
    {
        int MaxPaintOver { get; }
        bool IsPaintOverMax();
    }
}
