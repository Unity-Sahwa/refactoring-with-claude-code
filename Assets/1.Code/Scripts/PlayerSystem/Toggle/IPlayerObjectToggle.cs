namespace Refactoring
{
    // 책임: 핸들러가 어떤 대상을 어떤 상태로 바꿀지 알기 위한 계약. (일회성 설정이라 되돌리지 않는다)
    public interface IPlayerObjectToggle
    {
        ToggleTargetKey Key { get; }
        // true면 켜기, false면 끄기
        bool Activate { get; }
    }
}
