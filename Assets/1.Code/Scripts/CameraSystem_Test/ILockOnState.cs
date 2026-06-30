namespace Refactoring
{
    // 락온 여부 읽기용. 입력 해석(락온 대시)·이동 애니메이션 등이 구독한다.
    // 지금은 입력 처리기가 구현하고, 추후 카메라 시스템이 소유를 인수한다.
    public interface ILockOnState
    {
        bool IsLockOn { get; }
    }
}
