using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 설정창에서 고른 마우스 감도를 카메라 회전 속도에 반영한다. (회전 축을 쥔 유일한 곳이라 포인터 회전 차단도 맡는다)
    // 흐름: 프리팹 기준 감도 저장 → 설정이 바뀌면 곱해서 적용 → 컷씬이면 회전 입력 차단
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class MouseSpeedApplier : MonoBehaviour, IPointerLookControl
    {
        // 카메라 프리팹에 적힌 축 이름과 정확히 같아야 한다.
        private const string AxisX = "Look Orbit X";
        private const string AxisY = "Look Orbit Y";

        // 이 경로만 껐다 켜서 게임패드 회전은 살려둔다.
        private const string PointerDeltaPath = "<Pointer>/delta";

        [Preserve, Inject(true)] private IMouseSettings _mouseSettings;
        [Preserve, Inject(true)] private IGameStateProvider _gameState;

        private CinemachineInputAxisController _controller;

        // 프리팹에 원래 적힌 값. 상하 축은 값이 음수라 부호를 살려야 해서 그대로 두고 곱하기만 한다.
        private float _baseGainX;
        private float _baseGainY;

        // 다른 컴포넌트의 Start가 SetPointerLookEnabled를 먼저 부를 수 있어, 컨트롤러 참조는 Awake에 잡는다.
        private void Awake()
        {
            _controller = GetComponent<CinemachineInputAxisController>();
        }

        // Awake가 아니라 Start인 이유: 주입이 Awake에 일어나서, Awake에 읽으면 아직 비어 있다.
        private void Start()
        {
            _baseGainX = GetGain(AxisX);
            _baseGainY = GetGain(AxisY);

            if (_mouseSettings != null)
            {
                _mouseSettings.OnChanged += ApplySettings;
                ApplySettings();
            }

            if (_gameState != null)
            {
                _gameState.OnChanged += HandleStateChanged;
                HandleStateChanged(_gameState.Current);
            }
        }

        private void OnDestroy()
        {
            if (_mouseSettings != null)
            {
                _mouseSettings.OnChanged -= ApplySettings;
            }

            if (_gameState != null)
            {
                _gameState.OnChanged -= HandleStateChanged;
            }
        }

        // 빈 문자열을 덮어씌우면 그 경로가 꺼지고, null을 넣으면 원래 경로로 돌아간다.
        public void SetPointerLookEnabled(bool enabled)
        {
            string overridePath = enabled ? null : "";

            ApplyPointerOverride(AxisX, overridePath);
            ApplyPointerOverride(AxisY, overridePath);
        }

        // 컷씬 중엔 카메라 회전 입력을 꺼서 시점을 고정한다.
        private void HandleStateChanged(GameStateType state)
        {
            _controller.enabled = state != GameStateType.Cutscene;
        }

        private void ApplySettings()
        {
            SetGain(AxisX, _baseGainX * _mouseSettings.SpeedX);
            SetGain(AxisY, _baseGainY * _mouseSettings.SpeedY);
        }

        private void ApplyPointerOverride(string axisName, string overridePath)
        {
            InputAction action = _controller.GetController(axisName)?.Input?.InputAction?.action;

            if (action == null)
            {
                Debug.LogWarning($"{name}: '{axisName}' 축에 물린 입력 액션을 찾지 못함.");
                return;
            }

            action.ApplyBindingOverride(overridePath, path: PointerDeltaPath);
        }

        private float GetGain(string axisName)
        {
            CinemachineInputAxisController.Controller controller = _controller.GetController(axisName);
            return controller == null ? 0f : controller.Input.Gain;
        }

        private void SetGain(string axisName, float gain)
        {
            CinemachineInputAxisController.Controller controller = _controller.GetController(axisName);

            if (controller == null)
            {
                Debug.LogWarning($"{name}: '{axisName}' 축을 찾지 못함. 카메라 프리팹의 축 이름을 확인하세요.");
                return;
            }

            controller.Input.Gain = gain;
        }
    }
}
