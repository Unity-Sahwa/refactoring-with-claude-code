using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 플레이어 체력의 단일 출처. 증감만 담당하고, 값이 바뀌면 알린다(UI 등이 구독).
    // 대원_TODO: MaxHealth를 데이터 묶음에서 Inject로 받아오기
    public class Health : MonoBehaviour, IHealthInfo, IHealthModifier
    {
        [Tooltip("시작 최대 체력")]
        [SerializeField]
        private float _maxHealth = 3f;

        public float Current { get; private set; }
        public float Max { get; private set; }
        public event Action<float> OnChanged;

        // Start가 아니라 Awake인 이유: 다른 컴포넌트가 Start에서 Current를 읽어도 이미 값이 차 있어야 한다.
        private void Awake()
        {
            Setup(_maxHealth);
        }

        public void Setup(float maxHealth)
        {
            Max = maxHealth;
            Current = maxHealth;
            OnChanged?.Invoke(Current);
        }

        // 저장 슬롯을 불러올 때 그 슬롯에 적힌 체력으로 맞춘다.
        public void SetCurrent(float value)
        {
            Current = Mathf.Clamp(value, 0f, Max);
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
