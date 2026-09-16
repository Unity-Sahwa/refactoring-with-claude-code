using TMPro;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Refactoring
{
    // 책임: 마우스 감도 슬라이더 하나. 상하인지 좌우인지만 고르고, 값은 감도 주인에게 쓴다.
    [RequireComponent(typeof(Slider))]
    public class MouseSpeedSlider : MonoBehaviour
    {
        [SerializeField] private bool _isVertical;

        [SerializeField] private TextMeshProUGUI _sliderValueText;

        [Preserve, Inject(true)] private IMouseSettings _mouseSettings;

        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnEnable()
        {
            if (_mouseSettings == null)
            {
                return;
            }

            // 창을 열 때 현재 값으로 손잡이만 맞춘다(알림은 안 쏘게).
            float value = _isVertical ? _mouseSettings.SpeedY : _mouseSettings.SpeedX;
            _slider.SetValueWithoutNotify(value);
            ChangeValueText(value);
        }

        private void HandleValueChanged(float value)
        {
            if (_mouseSettings == null)
            {
                return;
            }

            if (_isVertical)
            {
                _mouseSettings.SpeedY = value;
            }
            else
            {
                _mouseSettings.SpeedX = value;
            }

            ChangeValueText(value);
        }

        private void ChangeValueText(float value)
        {
            if (_sliderValueText != null)
            {
                _sliderValueText.text = value.ToString("F1");
            }
        }
    }
}
