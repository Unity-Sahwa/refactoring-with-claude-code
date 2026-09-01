using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 기본 카메라가 바라보는 각도를 정해둔 값으로 맞춘다. 플레이어를 옮기는 건 EventPlayerWarp가 한다.
    public class EventStartView : EventData
    {
        [SerializeField] private float _horizontal;    // 좌우 각도
        [SerializeField] private float _vertical;      // 상하 각도

        [Preserve, Inject(true)] private List<CameraRole> _roles;

        public override void Execute()
        {
            CinemachineOrbitalFollow orbit = FindDefaultOrbit();

            if (orbit == null)
            {
                return;
            }

            orbit.HorizontalAxis.Value = _horizontal;
            orbit.VerticalAxis.Value = _vertical;
        }

        private CinemachineOrbitalFollow FindDefaultOrbit()
        {
            if (_roles == null)
            {
                return null;
            }

            foreach (CameraRole role in _roles)
            {
                if (role != null && role.Kind == CameraKind.Default && role.Camera != null)
                {
                    return role.Camera.GetComponent<CinemachineOrbitalFollow>();
                }
            }

            return null;
        }
    }
}
