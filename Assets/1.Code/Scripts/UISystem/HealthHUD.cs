using TMPro;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 체력이 바뀌면 하트 아이콘 개수와 숫자 표시를 맞춘다. 값 계산은 Health가 한다.
    public class HealthHUD : MonoBehaviour
    {
        [Preserve, Inject] private IHealthInfo _health;

        [Tooltip("하트 아이콘들. 순서대로 왼쪽부터 채워진다.")]
        [SerializeField] private GameObject[] _hpIcons;

        [Tooltip("\"20/20\" 형태로 찍히는 텍스트")]
        [SerializeField] private TMP_Text _text;

        // Awake가 아니라 Start인 이유: Health.Awake에서 발동한 OnChanged를 놓치므로 여기서 현재값으로 한 번 맞춘다.
        private void Start()
        {
            if (_health == null) return;

            _health.OnChanged += Refresh;
            Refresh(_health.Current);
        }

        private void OnDestroy()
        {
            if (_health != null) _health.OnChanged -= Refresh;
        }

        private void Refresh(float current)
        {
            for (int i = 0; i < _hpIcons.Length; i++)
            {
                _hpIcons[i].SetActive(i < current);
            }

            if (_text != null) _text.text = $"{current:0}/{_health.Max:0}";
        }
    }
}
