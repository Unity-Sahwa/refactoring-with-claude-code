namespace Refactoring
{
    // 책임: 지금 고른 슬롯 번호를 알려준다는 것만 약속한다.
    public interface ISlotSelection
    {
        int SelectedIndex { get; }
    }
}
