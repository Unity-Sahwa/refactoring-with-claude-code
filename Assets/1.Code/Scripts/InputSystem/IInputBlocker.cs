// 입력 전체 차단 여부를 외부에서 제공하는 인터페이스.
// GameManager가 구현 및 IInjectTarget 등록. InputEventBroadcaster가 DI로 주입받아 차단 여부 조회.
namespace Refactoring
{
    public interface IInputBlocker
    {
        bool IsInputBlocked { get; }
    }
}
