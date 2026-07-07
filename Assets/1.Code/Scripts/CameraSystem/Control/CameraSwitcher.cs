using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 역할: 구독 이벤트의 알람을 받고 카메라를 전환시킨다.(셋팅 정보 포함)
    // 우선순위: 타임라인 카메라 > 락온 카메라 > 디폴트 카메라. 타임라인은 개별적으로 
    public class CameraSwitcher : MonoBehaviour
    {
        private const int Active = 20;  // 켤 카메라 우선순위
        private const int Idle = 10;    // 나머지 카메라 우선순위

        [Inject] private List<CameraRole> _roles;                  // 씬의 역할표식 카메라들
        [Inject] private ICharacterSwapNotifier _swapNotifier;
        [Inject] private ICurrentCharacterProvider _currentCharacter;
        [Inject] private ILockOnState _lockOn;

        private readonly Dictionary<CameraKind, CinemachineCamera> _cameras = new();

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

            if (_swapNotifier != null) _swapNotifier.OnCharacterSwapped += OnChanged;
            if (_lockOn != null) _lockOn.OnLockOnChanged += OnChanged;
        }

        private void OnDestroy()
        {
            if (_swapNotifier != null) _swapNotifier.OnCharacterSwapped -= OnChanged;
            if (_lockOn != null) _lockOn.OnLockOnChanged -= OnChanged;
        }

        private void Start() => Refresh();

        private void OnChanged() => Refresh();

        // 락온이면 락온 카메라, 아니면 디폴트를 켜고 따라갈 대상을 연결한다.
        private void Refresh()
        {
            bool lockOn = _lockOn != null && _lockOn.IsLockOn && _cameras.ContainsKey(CameraKind.LockOn);
            CameraKind chosen = lockOn ? CameraKind.LockOn : CameraKind.Default;

            foreach (KeyValuePair<CameraKind, CinemachineCamera> entry in _cameras)
            {
                entry.Value.Priority.Value = entry.Key == chosen ? Active : Idle;
            }

            PlayerCharacter character = _currentCharacter?.CurrentCharacter;

            // 디폴트(=보스룸) 카메라는 현재 캐릭터를 따라간다.
            if (character != null && _cameras.TryGetValue(CameraKind.Default, out CinemachineCamera defaultCam))
            {
                defaultCam.Target.TrackingTarget = character.transform;
            }

            // 락온 카메라는 현재 캐릭터를 따라가고 또 바라본다(적이 아니라 플레이어).
            if (_cameras.TryGetValue(CameraKind.LockOn, out CinemachineCamera lockOnCam))
            {
                if (character != null)
                {
                    lockOnCam.Target.TrackingTarget = character.transform;
                    lockOnCam.Target.LookAtTarget = character.transform;
                }
            }
        }
    }
}
