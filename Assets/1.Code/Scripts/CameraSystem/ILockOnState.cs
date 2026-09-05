using System;

namespace Refactoring
{
    // 책임: 락온이 켜졌는지와 그 변화를 읽기 전용으로 알려준다. (대상이 누구인지는 ILockOnTarget 담당)
    public interface ILockOnState
    {
        bool IsLockOn { get; }
        event Action OnLockOnChanged;
    }
}
