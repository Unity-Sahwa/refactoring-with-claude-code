using UnityEngine;

namespace Refactoring
{
    // 역할: 플레이어 체력이 깎이면 이 UI를 잠깐 흔들고 제자리로 되돌린다.
    [RequireComponent(typeof(RectTransform))]
    public class UIShake : MonoBehaviour
    {
        [Tooltip("흔들리는 시간(초)")]
        [SerializeField]
        private float _shakeTime = 0.2f;

        [Tooltip("흔들리는 크기(픽셀)")]
        [SerializeField]
        private float _power = 10f;

        [Inject]
        private IHealthInfo _health;

        private RectTransform _rect;
        private Vector2 _startPosition;
        private float _timeLeft;
        private float _lastHealth;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _startPosition = _rect.anchoredPosition;
        }

        private void Start()
        {
            if (_health == null)
            {
                return;
            }

            _lastHealth = _health.Current;
            _health.OnChanged += HandleHealthChanged;
        }

        private void Update()
        {
            if (_timeLeft <= 0f)
            {
                return;
            }

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                _rect.anchoredPosition = _startPosition;
                return;
            }

            // 남은 시간이 줄면 흔들림 폭도 같이 줄어서 자연스럽게 잦아든다.
            float power = _power * (_timeLeft / _shakeTime);
            _rect.anchoredPosition = _startPosition + Random.insideUnitCircle * power;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnChanged -= HandleHealthChanged;
            }
        }

        // 회복일 때는 흔들면 안 되므로 줄었을 때만 시작한다.
        private void HandleHealthChanged(float current)
        {
            if (current < _lastHealth)
            {
                _timeLeft = _shakeTime;
            }

            _lastHealth = current;
        }
    }
}
