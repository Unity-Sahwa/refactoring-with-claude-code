using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 락온에 쓸 수 있는 적 후보를 매 프레임 골라 둔다. (누구를 고정할지는 LockOnController 담당)
    // 흐름: 반경 검사 → 화면 안 판정 → 화면 안쪽 방향 판정 → 벽 가림 판정 → 후보 목록
    public class LockOnTargetDetector : MonoBehaviour, ILockOnTargetDetector
    {
        [Tooltip("플레이어 중심 탐지 반경 (m)")]
        [SerializeField] private float _detectRange = 20f;

        [Tooltip("탐지할 적 레이어")]
        [SerializeField] private LayerMask _targetMask;

        [Tooltip("시야를 막는 벽 레이어")]
        [SerializeField] private LayerMask _obstacleMask;

        [Tooltip("탐지 반경과 후보를 Gizmos로 표시")]
        [SerializeField] private bool _isDebugDraw;

        [Preserve, Inject] private ICurrentCharacterProvider _character;

        // 매 프레임 새로 할당하지 않도록 전부 재사용한다.
        private readonly Collider[] _hits = new Collider[32];
        private readonly Plane[] _frustum = new Plane[6];
        private readonly List<Collider> _candidates = new List<Collider>(32);

        private Camera _mainCamera;

        public IReadOnlyList<Collider> Candidates => _candidates;

        private void Update()
        {
            _candidates.Clear();

            Transform characterTransform = _character?.GetCurrentComponent<Transform>();
            if (characterTransform == null)
            {
                return;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_mainCamera == null)
            {
                return;
            }

            Vector3 playerPosition = characterTransform.position;
            Vector3 cameraPosition = _mainCamera.transform.position;
            Vector3 cameraForward = _mainCamera.transform.forward;

            // 화면 안 판정에 쓸 6면을 이번 프레임 카메라로 갱신한다.
            GeometryUtility.CalculateFrustumPlanes(_mainCamera, _frustum);

            int count = Physics.OverlapSphereNonAlloc(playerPosition, _detectRange, _hits, _targetMask);

            for (int i = 0; i < count; i++)
            {
                Collider collider = _hits[i];

                // 화면 안에 보이는 적만.
                if (!GeometryUtility.TestPlanesAABB(_frustum, collider.bounds))
                {
                    continue;
                }

                // 플레이어 기준, 카메라가 바라보는 방향에 있는 적만.
                if (Vector3.Dot(collider.bounds.center - playerPosition, cameraForward) <= 0f)
                {
                    continue;
                }

                // 카메라와 적 사이가 벽에 막히면 제외.
                if (Physics.Linecast(cameraPosition, collider.bounds.center, _obstacleMask))
                {
                    continue;
                }

                _candidates.Add(collider);
            }
        }

        private void OnDrawGizmos()
        {
            if (!_isDebugDraw)
            {
                return;
            }

            Transform characterTransform = _character?.GetCurrentComponent<Transform>();
            if (characterTransform == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(characterTransform.position, _detectRange);

            Gizmos.color = Color.red;
            for (int i = 0; i < _candidates.Count; i++)
            {
                Collider collider = _candidates[i];
                if (collider != null)
                {
                    Gizmos.DrawWireSphere(collider.bounds.center, 0.5f);
                }
            }
        }
    }
}
