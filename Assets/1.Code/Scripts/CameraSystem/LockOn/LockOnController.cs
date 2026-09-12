using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 락온 입력을 받아 대상을 고정·유지·해제한다. (후보를 거르는 일은 LockOnTargetDetector 담당)
    // 흐름: 입력 → 후보 중 화면 중앙에 가까운 적 선택 → 고정 → 죽거나 멀어지면 교체·해제
    public class LockOnController : MonoBehaviour, ILockOnState, ILockOnTarget
    {
        [Tooltip("이 거리보다 멀어지면 락온이 풀린다 (m)")]
        [SerializeField] private float _releaseDistance = 20f;

        [Preserve, Inject] private IInputPressedProvider _inputPressedProvider;
        [Preserve, Inject] private ILockOnTargetDetector _lockOnTargetDetector;
        [Preserve, Inject] private ICurrentCharacterProvider _currentCharacterProvider;

        private Camera _mainCamera;

        public bool IsLockOn { get; private set; }
        public Collider LockedTarget { get; private set; }

        // 지금 조준 중인 적. 락온 전이면 "누르면 잡힐 후보", 락온 중이면 고정된 적. 없으면 null.
        public Collider AimTarget => IsLockOn ? LockedTarget : PickBest();

        public event Action OnLockOnChanged;

        private void Awake()
        {
            if (_inputPressedProvider != null)
            {
                _inputPressedProvider.OnInputPressed += HandleInputPressed;
            }
        }

        private void Update()
        {
            if (!IsLockOn)
            {
                return;
            }

            // 고정된 적이 사라졌거나 죽었으면: 새 후보가 있으면 갈아타고, 없으면 해제한다.
            if (LockedTarget == null || IsDead(LockedTarget))
            {
                Collider next = PickBest();
                if (next == null)
                {
                    Release();
                    return;
                }

                SetHighlight(LockedTarget, false);
                LockedTarget = next;
                SetHighlight(LockedTarget, true);
                OnLockOnChanged?.Invoke();
                return;
            }

            if (GetPlayerDistance(LockedTarget) > _releaseDistance)
            {
                Release();
            }
        }

        private void OnDestroy()
        {
            if (_inputPressedProvider != null)
            {
                _inputPressedProvider.OnInputPressed -= HandleInputPressed;
            }
        }

        // 켜져 있으면 끄고, 꺼져 있으면 지금 조준 가능한 적이 있을 때만 켠다.
        private void HandleInputPressed(InputActionType type)
        {
            if (type != InputActionType.LockOn)
            {
                return;
            }

            if (IsLockOn)
            {
                Release();
                return;
            }

            Collider target = PickBest();
            if (target == null)
            {
                return;
            }

            LockedTarget = target;
            IsLockOn = true;
            SetHighlight(LockedTarget, true);
            OnLockOnChanged?.Invoke();
        }

        private void Release()
        {
            SetHighlight(LockedTarget, false);
            IsLockOn = false;
            LockedTarget = null;
            OnLockOnChanged?.Invoke();
        }

        // static인 이유: 대상 콜라이더만 있으면 되는 일이라 특정 인스턴스에 속하지 않는다.
        private static void SetHighlight(Collider target, bool isOn)
        {
            if (target == null)
            {
                return;
            }

            target.GetComponentInParent<OutlineHighlight>()?.SetOutline(isOn);
        }

        // 기믹이 부서지거나 적이 죽으면 콜라이더가 꺼지고 레이어가 Default로 돌아간다.
        // static인 이유: 대상 콜라이더만 있으면 되는 일이라 특정 인스턴스에 속하지 않는다.
        private static bool IsDead(Collider collider)
        {
            return !collider.enabled || collider.gameObject.layer == 0;
        }

        // 후보 중 화면 중앙에 가장 가까운 하나. 없으면 null.
        private Collider PickBest()
        {
            IReadOnlyList<Collider> candidates = _lockOnTargetDetector?.Candidates;
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_mainCamera == null)
            {
                return null;
            }

            Collider best = null;
            float bestScore = Mathf.Infinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                Collider collider = candidates[i];
                if (collider == null)
                {
                    continue;
                }

                Vector3 viewportPoint = _mainCamera.WorldToViewportPoint(collider.bounds.center);
                float offsetX = viewportPoint.x - 0.5f;
                float offsetY = viewportPoint.y - 0.5f;

                // 크기 비교만 하므로 제곱근을 생략한다.
                float score = offsetX * offsetX + offsetY * offsetY;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = collider;
                }
            }

            return best;
        }

        // 높이 차이로 락온이 풀리지 않도록 y를 뺀 수평 거리만 잰다.
        private float GetPlayerDistance(Collider collider)
        {
            Transform characterTransform = _currentCharacterProvider?.GetCurrentComponent<Transform>();
            if (characterTransform == null)
            {
                return Mathf.Infinity;
            }

            Vector3 playerPosition = characterTransform.position;
            playerPosition.y = 0f;

            Vector3 targetPosition = collider.bounds.center;
            targetPosition.y = 0f;

            return Vector3.Distance(playerPosition, targetPosition);
        }
    }
}
