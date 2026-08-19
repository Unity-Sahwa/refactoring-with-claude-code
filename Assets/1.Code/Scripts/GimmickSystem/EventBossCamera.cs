using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 역할: 보스전 진입 시 디폴트 카메라의 궤도(OrbitalFollow Top/Center/Bottom)와
    //       락온 카메라의 FollowOffset/RotationComposer TargetOffset을 보스전용 값으로 바꾼다.
    public class EventBossCamera : EventData
    {
        [Header("Default Camera (OrbitalFollow)")]
        [SerializeField] private Cinemachine3OrbitRig.Orbit _bossTop;
        [SerializeField] private Cinemachine3OrbitRig.Orbit _bossCenter;
        [SerializeField] private Cinemachine3OrbitRig.Orbit _bossBottom;

        [Header("LockOn Camera")]
        [SerializeField] private Vector3 _bossFollowOffset;
        [SerializeField] private Vector3 _bossTargetOffset;

        [Inject] private List<CameraRole> _roles;

        public override void Execute()
        {
            if (FindComponent<CinemachineOrbitalFollow>(CameraKind.Default) is { } orbital)
            {
                // SplineCurvature는 건드리지 않고 Top/Center/Bottom만 바꾼다.
                Cinemachine3OrbitRig.Settings orbits = orbital.Orbits;
                orbits.Top = _bossTop;
                orbits.Center = _bossCenter;
                orbits.Bottom = _bossBottom;
                orbital.Orbits = orbits;
            }

            if (FindComponent<CinemachineFollow>(CameraKind.LockOn) is { } follow)
            {
                follow.FollowOffset = _bossFollowOffset;
            }

            if (FindComponent<CinemachineRotationComposer>(CameraKind.LockOn) is { } composer)
            {
                composer.TargetOffset = _bossTargetOffset;
            }
        }

        private T FindComponent<T>(CameraKind kind) where T : Component
        {
            if (_roles == null) return null;

            foreach (CameraRole role in _roles)
            {
                if (role != null && role.Kind == kind && role.Camera != null)
                {
                    role.Camera.TryGetComponent(out T component);
                    return component;
                }
            }
            return null;
        }
    }
}
