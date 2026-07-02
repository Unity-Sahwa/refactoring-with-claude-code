using UnityEngine;

namespace Refactoring
{
    // C: 락온(적 고정 조준) "상태"만 책임진다.
    // 방향 2(구독): 카메라·애니·TargetGroup을 직접 건드리지 않는다.
    //   여긴 "락온이 켜졌나(IsLockOn)"와 "고정된 적이 누구냐(LockedTarget)"만 내놓고,
    //   카메라·애니가 그 상태를 구독해 스스로 반응한다.
    public class LockOnController_Test : MonoBehaviour, ILockOnState, ILockOnTarget
    {
        // ponytail: 프로토타입 상수. 추후 CameraDetectData(SO, Refactoring)로 이주.
        private const float ReleaseDistance = 20f;  // 고정된 적이 이보다 멀어지면 자동 해제

        [Inject] private IInputPressedProvider _input;          // 락온 버튼 입력
        [Inject] private ITargetDetector _detector;             // 지금 조준 가능한 적(B)
        [Inject] private ICurrentCharacterProvider _character;  // 거리 계산용 플레이어 위치

        public bool IsLockOn { get; private set; }
        public Collider LockedTarget { get; private set; }

        private void Awake()
        {
            if (_input != null) _input.OnInputPressed += OnPressed;
        }

        private void OnDestroy()
        {
            if (_input != null) _input.OnInputPressed -= OnPressed;
        }

        // 락온 버튼: 켜져 있으면 끄고, 꺼져 있으면 "지금 조준 가능한 적"이 있을 때만 켠다.
        private void OnPressed(InputActionType type)
        {
            if (type != InputActionType.LockOn) return;

            if (IsLockOn)
            {
                Release();
                return;
            }

            Collider target = _detector?.CurrentTarget;
            if (target == null) return;     // 잡을 적이 없으면 락온 안 켜짐

            LockedTarget = target;
            IsLockOn = true;
        }

        private void Update()
        {
            if (!IsLockOn) return;

            // 고정된 적이 사라졌거나(파괴) 죽었으면: 주변에 새 후보가 있으면 갈아타고, 없으면 해제.
            if (LockedTarget == null || IsDead(LockedTarget))
            {
                Collider next = _detector?.CurrentTarget;
                if (next != null) LockedTarget = next;
                else Release();
                return;
            }

            // 너무 멀어지면 해제.
            if (PlayerDistance(LockedTarget) > ReleaseDistance)
            {
                Release();
            }
        }

        private void Release()
        {
            IsLockOn = false;
            LockedTarget = null;
        }

        // ponytail: 원본 Enemy(글로벌 네임스페이스)의 isDead 임시 참조. 적 상태 인터페이스 정리 시 교체.
        //           원본의 hpBarCount 등 세부 죽음 판정은 보류.
        private static bool IsDead(Collider c)
        {
            //Enemy enemy = c.GetComponent<Enemy>();
            //return enemy == null || enemy.isDead;
            return false;
        }

        // y(높이)를 무시한 수평 거리.
        private float PlayerDistance(Collider c)
        {
            if (_character?.CurrentCharacter == null) return Mathf.Infinity;
            Vector3 a = _character.CurrentCharacter.transform.position; a.y = 0;
            Vector3 b = c.transform.position; b.y = 0;
            return Vector3.Distance(a, b);
        }
    }
}
