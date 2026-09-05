using System;

namespace Refactoring
{
    // 체력 읽기용. UI 등이 현재/최대 체력과 변경 알림을 구독한다.
    public interface IHealthInfo
    {
        float Current { get; }
        float Max { get; }
        event Action<float> OnChanged;
    }
}
