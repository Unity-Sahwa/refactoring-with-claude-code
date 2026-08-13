using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 플레이어와 카메라가 원하는 곳을 바라보게 맞춘다.
    // 트리거로 부르거나, 씬이 시작할 때 바로 맞추려면 _executeOnStart를 켠다.
    public class EventStartView : EventData
    {
        [SerializeField] private bool _executeOnStart = false;

        [Header("플레이어")]
        [SerializeField] private bool _setPlayer = true;
        [SerializeField] private Vector3 _playerPosition;
        [SerializeField] private Vector3 _playerRotation;

        [Header("카메라")]
        [SerializeField] private bool _setCamera = true;
        [SerializeField] private float _horizontal;    // 좌우 각도
        [SerializeField] private float _vertical;      // 상하 각도

        [Inject(true)] private ICurrentCharacterProvider _character;
        [Inject(true)] private List<CameraRole> _roles;

        private void Start()
        {
            if (_executeOnStart)
            {
                Execute();
            }
        }

        public override void Execute()
        {
            if (_setPlayer && _character?.CurrentCharacter != null)
            {
                Transform player = _character.CurrentCharacter.transform;
                player.position = _playerPosition;
                player.rotation = Quaternion.Euler(_playerRotation);
            }

            if (_setCamera)
            {
                CinemachineOrbitalFollow orbit = FindDefaultOrbit();

                if (orbit != null)
                {
                    orbit.HorizontalAxis.Value = _horizontal;
                    orbit.VerticalAxis.Value = _vertical;
                }
            }
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
