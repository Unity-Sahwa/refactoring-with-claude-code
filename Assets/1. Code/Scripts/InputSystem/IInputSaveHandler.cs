// 키 바인딩 저장·로드를 외부 저장 시스템에 위임하는 인터페이스.
// SaveManager가 구현. InputRebindingService가 DI로 주입받아 리바인딩 후 저장 요청.
// Unity Input System의 전체 override JSON을 저장/로드.
namespace Refactoring
{
    public interface IInputSaveHandler
    {
        void SaveBindings(string overrideJson);
        string LoadBindings();
    }
}
