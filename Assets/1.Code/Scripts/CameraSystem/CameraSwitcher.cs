using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 캐릭터 교체·락온 알림을 받아 켤 카메라를 고르고 따라갈 대상을 연결한다.
    // 흐름: 알림 수신 → 켤 종류 결정 → 우선순위 부여 → 추적 대상 연결
    public class CameraSwitcher : MonoBehaviour, ICurrentCameraProvider
    {
        // 타임라인·컷씬 카메라는 이 둘보다 높은 우선순위를 자기가 직접 올려 쓴다.
        private const int Active = 20;
        private const int Idle = 10;

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
                entry.Value.Priority.Value = entry.Key == chosen ? Active : Idle;
            }

            _cameras.TryGetValue(chosen, out CinemachineCamera activeCamera);
            ActiveCamera = activeCamera;

            Transform characterTransform = _currentCharacter?.GetCurrentComponent<Transform>();
            if (characterTransform == null)
            {
                return;
            }

            // 락온 카메라도 적이 아니라 플레이어를 따라가고 바라본다.
            foreach (KeyValuePair<CameraKind, CinemachineCamera> entry in _cameras)
            {
                entry.Value.Target.TrackingTarget = characterTransform;
                entry.Value.Target.LookAtTarget = characterTransform;
            }
        }
    }
}
