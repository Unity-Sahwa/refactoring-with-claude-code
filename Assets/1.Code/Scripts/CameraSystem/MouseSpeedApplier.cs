using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 설정창에서 고른 마우스 감도를 카메라 회전 속도에 반영한다.
    // CinemachineInputAxisController가 붙은 오브젝트에 같이 붙인다.
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class MouseSpeedApplier : MonoBehaviour
    {
        // 좌우/상하 축 이름. 카메라 프리팹에 적힌 이름과 정확히 같아야 한다.
        private const string AxisX = "Look Orbit X";
        private const string AxisY = "Look Orbit Y";

        [Inject(true)] private IMouseSettings _mouseSettings;

        private CinemachineInputAxisController _controller;

        // 프리팹에 원래 적힌 값. 상하 축은 값이 음수라 부호를 살려야 해서 그대로 두고 곱하기만 한다.
        private float _baseGainX;
        private float _baseGainY;

        // Awake가 아니라 Start인 이유: 주입이 Awake에 일어나서, Awake에 읽으면 아직 비어 있다.
        private void Start()
        {
            _controller = GetComponent<CinemachineInputAxisController>();
            _baseGainX = GetGain(AxisX);
            _baseGainY = GetGain(AxisY);

            if (_mouseSettings == null)
            {
                return;
            }

            _mouseSettings.OnChanged += ApplySettings;
            ApplySettings();
        }

        private void OnDestroy()
        {
            if (_mouseSettings != null)
            {
                _mouseSettings.OnChanged -= ApplySettings;
            }
        }

        private void ApplySettings()
        {
            SetGain(AxisX, _baseGainX * _mouseSettings.SpeedX);
            SetGain(AxisY, _baseGainY * _mouseSettings.SpeedY);
        }

        private float GetGain(string axisName)
        {
            var controller = _controller.GetController(axisName);
            return controller == null ? 0f : controller.Input.Gain;
        }

        private void SetGain(string axisName, float gain)
        {
            var controller = _controller.GetController(axisName);

            if (controller == null)
            {
                Debug.LogWarning($"{name}: '{axisName}' 축을 찾지 못함. 카메라 프리팹의 축 이름을 확인하세요.");
                return;
            }

            controller.Input.Gain = gain;
        }
    }
}
