using System;

namespace Refactoring
{
    // 락온 상태 변화 읽기용.
    public interface ILockOnState
    {
        bool IsLockOn { get; }
        event Action OnLockOnChanged;
    }
}
