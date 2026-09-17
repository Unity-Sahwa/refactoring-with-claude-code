namespace Refactoring
{
    // 책임: 메뉴 UI 총괄에게 창 열기/닫기만 요청할 수 있다는 것을 약속한다.
    public interface IUIRoot
    {
        void OpenWindow(WindowType id);
        void CloseTop();
    }
}
