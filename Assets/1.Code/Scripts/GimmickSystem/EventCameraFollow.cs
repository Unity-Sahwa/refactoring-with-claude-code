using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 역할: 지정한 종류의 카메라가 대상을 따라가지 않게 끊거나, 다시 되돌린다.
    public class EventCameraFollow : EventData
    {
        [SerializeField] private CameraKind kind = CameraKind.Default;
        [SerializeField] private bool follow = false;

        [Inject] private List<CameraRole> _roles;

        private Transform _saved;

        public override void Execute()
        {
            CinemachineCamera cam = Find();
            if (cam == null) return;

            if (follow)
            {
                if (_saved == null) return;
                cam.Target.TrackingTarget = _saved;
                _saved = null;
            }
            else
            {
                // 이동만 멈춘다. 시선은 LookAtTarget이 그대로 유지한다.
                _saved = cam.Target.TrackingTarget;
                if (_saved == null) return;

                cam.Target.TrackingTarget = null;
            }
        }

        private CinemachineCamera Find()
        {
            if (_roles == null) return null;

            foreach (CameraRole role in _roles)
            {
                if (role != null && role.Kind == kind) return role.Camera;
            }
            return null;
        }
    }
}
