using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 마우스 감도 슬라이더 하나. 상하인지 좌우인지만 고르고, 값은 감도 주인에게 쓴다.
    [RequireComponent(typeof(Slider))]
    public class MouseSpeedSlider : MonoBehaviour
    {
        [SerializeField]
        private bool _isVertical;

        [Inject(true)] private IMouseSettings _mouseSettings;

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
            _slider.SetValueWithoutNotify(_isVertical ? _mouseSettings.SpeedY : _mouseSettings.SpeedX);
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
        }
    }
}
