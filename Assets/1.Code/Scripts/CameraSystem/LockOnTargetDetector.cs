using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 조건에 맞는 적을 매 프레임 탐지

    public class LockOnTargetDetector : MonoBehaviour, ILockOnTargetDetector
    {
        [SerializeField] private float _detectRange = 20f;      // 플레이어 중심 탐지 반경
        [SerializeField] private LayerMask _targetMask;         // 탐지할 적 레이어
        [SerializeField] private LayerMask _obstacleMask;       // 시야를 막는 벽 레이어
        [SerializeField] private bool _debug;                   // Gizmos 디버그 표시 on/off

        [Inject] private ICurrentCharacterProvider _character;  // 플레이어 위치

        public IReadOnlyList<Collider> Candidates => _candidates;

        // 매 프레임 새로 만들지 않도록 재사용.
        private readonly Collider[] _hits = new Collider[32];
        private readonly Plane[] _frustum = new Plane[6];       // 절두체 6면 재사용
        private readonly List<Collider> _candidates = new List<Collider>(32);
        private Camera _mainCamera;

        private void Update()
        {
            _candidates.Clear();

            if (_character?.CurrentCharacter == null) return;

            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector3 playerPosition = _character.CurrentCharacter.transform.position;
            Vector3 cameraPosition = _mainCamera.transform.position;
            Vector3 cameraForward = _mainCamera.transform.forward;

            // 화면 안(절두체) 판정에 쓸 6면을 이번 프레임 카메라로 갱신.
            GeometryUtility.CalculateFrustumPlanes(_mainCamera, _frustum);

            int count = Physics.OverlapSphereNonAlloc(playerPosition, _detectRange, _hits, _targetMask);

            for (int i = 0; i < count; i++)
            {
                Collider collider = _hits[i];

                // 화면 안에 보이는 적만.
                if (!GeometryUtility.TestPlanesAABB(_frustum, collider.bounds)) continue;

                // 플레이어 기준, 카메라가 바라보는 방향(화면 안쪽)에 있는 적만.
                if (Vector3.Dot(collider.bounds.center - playerPosition, cameraForward) <= 0f) continue;

                // 카메라와 적 사이가 벽에 막히면 제외.
                if (Physics.Linecast(cameraPosition, collider.bounds.center, _obstacleMask)) continue;

                _candidates.Add(collider);
            }
        }

        private void OnDrawGizmos()
        {
            if (!_debug) return;
            if (_character?.CurrentCharacter == null) return;

            // 탐지 반경.
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_character.CurrentCharacter.transform.position, _detectRange);

            // 후보 전체 표시.
            Gizmos.color = Color.red;
            for (int i = 0; i < _candidates.Count; i++)
            {
                Collider collider = _candidates[i];
                if (collider != null) Gizmos.DrawWireSphere(collider.bounds.center, 0.5f);
            }
        }
    }
}
