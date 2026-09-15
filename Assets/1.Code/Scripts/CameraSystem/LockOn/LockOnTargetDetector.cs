using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

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

        [Preserve, Inject] private ICurrentCharacterProvider _character;

        // 매 프레임 새로 할당하지 않도록 전부 재사용한다.
        private readonly Collider[] _hits = new Collider[32];
        private readonly Plane[] _frustum = new Plane[6];
        private readonly List<Collider> _candidates = new List<Collider>(32);

        private Camera _mainCamera;

        public IReadOnlyList<Collider> Candidates => _candidates;

        private void Awake()
        {
            if (_character == null)
            {
                throw new InvalidOperationException($"{nameof(LockOnTargetDetector)}: 필수 의존 주입 실패");
            }
        }

        private void Update()
        {
            _candidates.Clear();

            if (!TryGetContext(out Transform characterTransform, out Camera mainCamera))
            {
                return;
            }

            CollectCandidates(characterTransform, mainCamera);
        }

        private bool TryGetContext(out Transform characterTransform, out Camera mainCamera)
        {
            characterTransform = _character.GetCurrentComponent<Transform>();

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
            mainCamera = _mainCamera;

            return characterTransform != null && mainCamera != null;
        }

        private void CollectCandidates(Transform characterTransform, Camera mainCamera)
        {
            Vector3 playerPosition = characterTransform.position;

            // 화면 안 판정에 쓸 6면을 이번 프레임 카메라로 갱신한다.
            GeometryUtility.CalculateFrustumPlanes(mainCamera, _frustum);

            int count = Physics.OverlapSphereNonAlloc(playerPosition, _detectRange, _hits, _targetMask);

            for (int i = 0; i < count; i++)
            {
                Collider collider = _hits[i];
                if (IsVisibleCandidate(collider, playerPosition, mainCamera))
                {
                    _candidates.Add(collider);
                }
            }
        }

        // 화면 안에 보이고, 카메라가 바라보는 방향에 있고, 벽에 막히지 않은 적만 후보로 인정한다.
        private bool IsVisibleCandidate(Collider collider, Vector3 playerPosition, Camera mainCamera)
        {
            if (!GeometryUtility.TestPlanesAABB(_frustum, collider.bounds))
            {
                return false;
            }

            if (Vector3.Dot(collider.bounds.center - playerPosition, mainCamera.transform.forward) <= 0f)
            {
                return false;
            }

            return !Physics.Linecast(mainCamera.transform.position, collider.bounds.center, _obstacleMask);
        }
    }
}
