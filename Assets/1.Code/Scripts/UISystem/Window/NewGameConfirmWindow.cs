namespace Refactoring
{
    // 책임: "새로 시작할까요?" 창. 예를 누르면 컷만화를 연다.
    public class NewGameConfirmWindow : ConfirmWindow
    {
        protected override void RunYes()
        {
            Root.OpenWindow(WindowType.StoryToon);
        }
    }
}
