using System;

namespace Refactoring
{
    // 책임: 락온 입력이 눌렸음을 알려준다. 구독자는 어떤 액션인지 몰라도 된다.
    public interface ILockOnInputProvider
    {
        event Action OnLockOnPressed;
    }
}
