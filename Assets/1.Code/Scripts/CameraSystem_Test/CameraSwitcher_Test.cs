using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // A: 락온 상태(C)를 폴링해서 카메라를 바꾼다. 전환은 Priority 방식(둘 다 켜두고 우선순위만 조정).
    // 방향 2: C는 A를 모른다. A가 C의 IsLockOn/LockedTarget을 "보고" 스스로 전환한다.
    // 범위: Default/LockOn 2개만. FinishSkill은 타임라인 연출로 분리, 락온은 TargetGroup 없이 LookAt만 사용.
    // Cinemachine 3.x: 두 카메라가 같은 CinemachineCamera 타입이라 CameraRole_Test 표식으로 구분한다.
    public class CameraSwitcher_Test : MonoBehaviour
    {
        private const int Active = 20;  // 활성 카메라 우선순위
        private const int Idle = 10;    // 대기 카메라 우선순위

        [Inject] private ILockOnState _lockOn;              // 락온 켜짐 여부(폴링)
        [Inject] private ILockOnTarget _lockOnTarget;       // 고정된 적(LookAt 대상)
        [Inject] private List<CameraRole_Test> _roles;      // 씬의 역할표식 카메라들(Default/LockOn)

        private CinemachineCamera _defaultCam;
        private CinemachineCamera _lockOnCam;
        private bool _prev;

        private void Start()
        {
            if (_roles != null)
            {
                foreach (var role in _roles)
                {
                    if (role.Kind == CameraKind.Default) _defaultCam = role.Camera;
                    else if (role.Kind == CameraKind.LockOn) _lockOnCam = role.Camera;
                }
            }

            bool on = _lockOn != null && _lockOn.IsLockOn;
            Apply(on);
            _prev = on;
        }

        private void Update()
        {
            if (_lockOn == null) return;

            bool now = _lockOn.IsLockOn;
            if (now != _prev)
            {
                Apply(now);
                _prev = now;
            }

            // 락온 중엔 대상이 바뀔 수 있으니(C가 자동으로 다음 적으로 갈아탐) LookAt을 계속 맞춘다.
            if (now && _lockOnCam != null && _lockOnTarget?.LockedTarget != null)
            {
                _lockOnCam.LookAt = _lockOnTarget.LockedTarget.transform;
            }
        }

        // 락온이면 락온 카메라를, 아니면 평소 카메라를 활성 우선순위로.
        private void Apply(bool lockOn)
        {
            if (_defaultCam != null) _defaultCam.Priority.Value = lockOn ? Idle : Active;
            if (_lockOnCam != null) _lockOnCam.Priority.Value = lockOn ? Active : Idle;
        }
    }
}
