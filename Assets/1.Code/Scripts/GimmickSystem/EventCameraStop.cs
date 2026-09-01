using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 역할: 지정한 종류의 카메라가 대상을 따라가지 않게 끊는다.
    public class EventCameraStop : EventData
    {
        [SerializeField] private CameraKind kind = CameraKind.Default;

        [Preserve, Inject] private List<CameraRole> _roles;

        public override void Execute()
        {
            CinemachineCamera cam = Find();
            if (cam == null) return;

            cam.Target.TrackingTarget = null;

            CinemachineDeoccluder deoccluder = cam.GetComponent<CinemachineDeoccluder>();
            if (deoccluder != null) deoccluder.enabled = false;
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
