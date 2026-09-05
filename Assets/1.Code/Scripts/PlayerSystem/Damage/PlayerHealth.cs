using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 플레이어 체력의 단일 출처. 증감만 담당하고, 값이 바뀌면 알린다(UI 등이 구독).
    public class PlayerHealth : MonoBehaviour, IHealthInfo, IHealthModifier
    {
        [Tooltip("시작 최대 체력")]
        [SerializeField] private float _maxHealth = 20f;

        public float Current { get; private set; }
        public float Max { get; private set; }
        public event Action<float> OnChanged;

        // 씬이 바뀔 때 체력을 들고 넘어가기 위한 값. 불러오기가 아닌 일반 씬 전환(스토리 진행 등)에도
        // 체력이 리셋되지 않게 하려고 둔다. 불러오기 때는 GameStateSaver.Restore가 sceneLoaded에서
        // 이 값보다 나중에 SetCurrent를 불러 덮어써서 충돌하지 않는다.
        // ponytail: 새 게임 시작을 초기화하는 진입점이 없어 이전 판 체력이 새 게임 첫 씬에 남을 수 있음.
        // 새 게임 시작 로직이 생기면 거기서 _carriedCurrent = null로 비워줄 것.
        private static float? _carriedCurrent;

        // Start가 아니라 Awake인 이유: 다른 컴포넌트가 Start에서 Current를 읽어도 이미 값이 차 있어야 한다.
        private void Awake()
        {
            Setup(_maxHealth);

            if (_carriedCurrent.HasValue)
            {
                SetCurrent(_carriedCurrent.Value);
                _carriedCurrent = null;
            }
        }

        private void OnDestroy()
        {
            _carriedCurrent = Current;
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
            if (amount <= 0f)
            {
                return Current;
            }

            Current = Mathf.Max(0f, Current - amount);
            OnChanged?.Invoke(Current);
            return Current;
        }
    }
}
