namespace Refactoring
{
    // 책임: 창 하나가 열리고 닫힐 수 있다는 것만 약속한다.
    public interface IWindow
    {
        void Open();
        void Close();
    }
}
