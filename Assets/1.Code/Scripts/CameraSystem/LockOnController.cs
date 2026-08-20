using System;
using UnityEngine;

namespace Refactoring
{
    public class LockOnController : MonoBehaviour, ILockOnState, ILockOnTarget
    {
        [SerializeField] private float _releaseDistance = 20f;

        [Inject] private IInputPressedProvider _inputPressedProvider;
        [Inject] private ILockOnTargetDetector _lockOnTargetDetector;
        [Inject] private ICurrentCharacterProvider _currentCharacterProvider;

        public bool IsLockOn { get; private set; }
        public Collider LockedTarget { get; private set; }

        // 지금 조준 중인 적. 락온 전이면 "누르면 잡힐 후보", 락온 중이면 고정된 적. 없으면 null.
        public Collider AimTarget => IsLockOn ? LockedTarget : PickBest();

        public event Action OnLockOnChanged;

        private Camera _mainCamera;

        private void Awake()
        {
            if (_inputPressedProvider != null) _inputPressedProvider.OnInputPressed += OnPressed;
        }

        private void OnDestroy()
        {
            if (_inputPressedProvider != null) _inputPressedProvider.OnInputPressed -= OnPressed;
        }

        // 락온 켜져 있으면 끄고, 꺼져 있으면 "지금 조준 가능한 적"이 있을 때만 켠다.
        private void OnPressed(InputActionType type)
        {
            if (type != InputActionType.LockOn) return;

            if (IsLockOn)
            {
                Release();
                return;
            }

            Collider target = PickBest();
            if (target == null) return;     // 잡을 적이 없으면 락온 안 켜짐

            LockedTarget = target;
            IsLockOn = true;
            SetHighlight(LockedTarget, true);
            OnLockOnChanged?.Invoke();

            Debug.Log($"target: {LockedTarget.gameObject.name}");
        }

        private void Update()
        {
            if (!IsLockOn) return;

            // 고정된 적이 사라졌거나(파괴) 죽었으면: 주변에 새 후보가 있으면 갈아타고, 없으면 해제.
            if (LockedTarget == null || IsDead(LockedTarget))
            {
                Collider next = PickBest();
                if (next != null)
                {
                    SetHighlight(LockedTarget, false);
                    LockedTarget = next;
                    SetHighlight(LockedTarget, true);
                    OnLockOnChanged?.Invoke();
                }
                else Release();
                return;
            }

            if (PlayerDistance(LockedTarget) > _releaseDistance)
            {
                Release();
            }
        }

        //락온 해제
        private void Release()
        {
            SetHighlight(LockedTarget, false);
            IsLockOn = false;
            LockedTarget = null;
            OnLockOnChanged?.Invoke();
        }

        // 락온 대상 아웃라인 표시 on/off. 대상에 OutlineHighlight가 없으면 아무 일도 안 함.
        private static void SetHighlight(Collider target, bool isOn)
        {
            if (target == null) return;
            target.GetComponentInParent<OutlineHighlight>()?.SetOutline(isOn);
        }

        //기믹이 파괴되거나 적이 죽은 상태를 반환
        private static bool IsDead(Collider collider)
        {
            return !collider.enabled || collider.gameObject.layer == 0;
        }

        // 후보 중 화면 중앙에 가장 가까운 1명(조준 대상). 없으면 null.
        private Collider PickBest()
        {
            var candidates = _lockOnTargetDetector?.Candidates;
            if (candidates == null || candidates.Count == 0) return null;

            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return null;

            Collider best = null;
            float bestScore = Mathf.Infinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                Collider collider = candidates[i];
                if (collider == null) continue;
                Vector3 viewportPoint = _mainCamera.WorldToViewportPoint(collider.bounds.center);
                float offsetX = viewportPoint.x - 0.5f;
                float offsetY = viewportPoint.y - 0.5f;
                float score = offsetX * offsetX + offsetY * offsetY; //단순거리 비교라 제곱근 X
                if (score < bestScore)
                {
                    bestScore = score;
                    best = collider;
                }
            }
            return best;
        }

        // y(높이)를 무시한 수평 거리.
        private float PlayerDistance(Collider collider)
        {
            if (_currentCharacterProvider?.CurrentCharacter == null) return Mathf.Infinity;
            Vector3 playerPosition = _currentCharacterProvider.CurrentCharacter.transform.position; playerPosition.y = 0;
            Vector3 targetPosition = collider.bounds.center; targetPosition.y = 0;
            return Vector3.Distance(playerPosition, targetPosition);
        }
    }
}
