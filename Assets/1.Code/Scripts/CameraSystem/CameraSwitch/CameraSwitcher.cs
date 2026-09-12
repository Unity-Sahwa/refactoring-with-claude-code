using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 캐릭터 교체·락온 알림을 받아 카메라를 전환하고 따라갈 대상을 연결한다.
    // 흐름: 알림 수신 → 전환될 카메라 결정 → 우선순위 부여 → 추적 대상 연결
    public class CameraSwitcher : MonoBehaviour, ICurrentCameraProvider
    {
        private const int PlayerActiveCameraOrder = 20;
        private const int PlayerIdleCameraOrder = 10;

        // 씬에 있는 역할 표식 카메라 전부.
        [Preserve, Inject] private List<CameraRole> _roles;
        [Preserve, Inject] private ICharacterSwapNotifier _swapNotifier;
        [Preserve, Inject] private ICurrentCharacterProvider _currentCharacter;
        [Preserve, Inject] private ILockOnState _lockOn;

        private readonly Dictionary<CameraKind, CinemachineCamera> _cameras = new();

        // 지금 켜진(Priority == Active) 가상 카메라. 카메라 연출(줌)이 이걸 대상으로 삼는다.
        public CinemachineCamera ActiveCamera { get; private set; }

        private void Awake()
        {
            if (_roles != null)
            {
                foreach (CameraRole role in _roles)
                {
                    if (role != null && role.Camera != null)
                    {
                        _cameras[role.Kind] = role.Camera;
                    }
                }
            }

            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped += HandleChanged;
            }

            if (_lockOn != null)
            {
                _lockOn.OnLockOnChanged += HandleChanged;
            }
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped -= HandleChanged;
            }

            if (_lockOn != null)
            {
                _lockOn.OnLockOnChanged -= HandleChanged;
            }
        }

        private void HandleChanged()
        {
            Refresh();
        }

        private void Refresh()
        {
            bool isLockOn = _lockOn != null && _lockOn.IsLockOn && _cameras.ContainsKey(CameraKind.LockOn);
            CameraKind chosen = isLockOn ? CameraKind.LockOn : CameraKind.Default;

            foreach (KeyValuePair<CameraKind, CinemachineCamera> entry in _cameras)
            {
                entry.Value.Priority.Value = entry.Key == chosen ? PlayerActiveCameraOrder : PlayerIdleCameraOrder;
            }

            _cameras.TryGetValue(chosen, out CinemachineCamera activeCamera);
            ActiveCamera = activeCamera;

            Transform characterTransform = _currentCharacter?.GetCurrentComponent<Transform>();
            if (characterTransform ==  null)
            {
                return;
            }

            // 락온, 디폴트 카메라 모두 플레이어를 할당함
            foreach (KeyValuePair<CameraKind, CinemachineCamera> entry in _cameras)
            {
                entry.Value.Target.TrackingTarget = characterTransform;
                entry.Value.Target.LookAtTarget = characterTransform;
            }
        }
    }
}
