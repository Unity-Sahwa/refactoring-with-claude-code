using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 플레이어 체력의 단일 출처. 증감만 담당하고, 값이 바뀌면 알린다(UI 등이 구독).
    // 대원_TODO: MaxHP를 데이터 묶음에서 Inject로 받아오기
    public class Health : MonoBehaviour, IHealthInfo, IHealthModifier
    {
        public float Current { get; private set; }
        public float Max { get; private set; }
        public event Action<float> OnChanged;

        public void Setup(float max)
        {
            Max = max;
            Current = max;
            OnChanged?.Invoke(Current);
        }

        public float Decrease(float amount)
        {
            if (amount <= 0f) return Current;

            Current = Mathf.Max(0f, Current - amount);
            OnChanged?.Invoke(Current);
            return Current;
        }
    }
}
