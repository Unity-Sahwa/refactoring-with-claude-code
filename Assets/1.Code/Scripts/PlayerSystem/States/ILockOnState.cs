namespace Refactoring
{
    // 락온 모드 조회용. 상태시스템은 이 인터페이스로 "지금 락온이냐"만 본다.
    // 실제 락온/카메라 타겟팅 구현은 상태시스템 밖(외부 시스템)에 있다.
    public interface ILockOnState
    {
        bool IsLockOn { get; }
    }
}
