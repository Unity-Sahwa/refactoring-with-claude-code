using System;
using UnityEngine;

namespace Refactoring
{
    // B: 매 프레임 주변 적을 훑어 "지금 조준할 적 하나"를 고른다.
    // 고르는 규칙(원본 보존): 아주 가까운(CloseDistance 이내) 적이 있으면 그중 "가장 가까운" 적,
    //                        없으면 화면 중앙에서 "가장 덜 벗어난(각도 최소)" 적.
    // 원본의 곁가지(HUD 페이드·가이드·머리위치 찾기)는 떼어냄 — 그건 마커/락온이 할 일.
    public class TargetDetector_Test : MonoBehaviour, ITargetDetector
    {
        // ponytail: 프로토타입 상수. 추후 CameraDetectData(SO, Refactoring)로 이주 예정.
        private const float DetectRange = 20f;   // 탐지 반경
        private const float MaxAngle = 60f;      // 화면 중앙(플레이어 방향)에서 허용하는 좌우 각도
        private const float MaxDistance = 60f;   // 조준 허용 최대 거리
        private const float CloseDistance = 5f;  // 이 안의 적은 "가까운 적 우선" 규칙을 탄다

        [Inject] private ICurrentCharacterProvider _character;  // 플레이어 위치
        [Inject] private ICameraPositionProvider _camera;       // 카메라 위치(각도 계산용)

        public Collider CurrentTarget { get; private set; }
        public event Action<Collider> OnTargetChanged;

        // 매 프레임 새 배열을 안 만들도록 재사용(OverlapSphereNonAlloc).
        private readonly Collider[] _hits = new Collider[32];
        // ponytail: 레이어 이름 하드코딩. TagManager에 "Enemy"(인덱스 8) 존재 확인함.
        private readonly int _enemyMask = LayerMask.GetMask("Enemy");

        private void Update()
        {
            Collider found = FindTarget();
            if (found != CurrentTarget)
            {
                CurrentTarget = found;
                OnTargetChanged?.Invoke(found);
            }
        }

        private Collider FindTarget()
        {
            if (_character?.CurrentCharacter == null) return null;

            Vector3 playerPos = _character.CurrentCharacter.transform.position;
            Vector3 cameraPos = _camera != null ? _camera.CameraPosition : playerPos;

            int count = Physics.OverlapSphereNonAlloc(playerPos, DetectRange, _hits, _enemyMask);

            Collider best = null;
            float bestDistance = CloseDistance;     // 가까운 적 후보(이 값보다 가까워야 채택)
            float bestAngle = Mathf.Infinity;       // 가까운 적이 없을 때 쓰는 각도 후보
            bool hasClose = false;

            for (int i = 0; i < count; i++)
            {
                Collider c = _hits[i];

                // ponytail: 원본 Enemy(글로벌 네임스페이스)의 isDead를 임시 참조.
                //           적 상태 인터페이스가 정리되면 그쪽으로 교체.
                Enemy enemy = c.GetComponent<Enemy>();
                if (enemy == null || enemy.isDead) continue;

                float angle = AngleFromCenter(c.transform.position, playerPos, cameraPos);
                float distance = FlatDistance(c.transform.position, playerPos);

                if (angle > MaxAngle) continue;
                if (distance > MaxDistance) continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = c;
                    hasClose = true;
                }
                else if (!hasClose && angle < bestAngle)
                {
                    bestAngle = angle;
                    best = c;
                }
            }

            return best;
        }

        // 카메라에서 본 "플레이어 방향" 대비 "타겟 방향"의 각도(수평면 기준).
        // = 화면 중앙(플레이어)에서 타겟이 좌우로 얼마나 벗어났나.
        private static float AngleFromCenter(Vector3 targetPos, Vector3 playerPos, Vector3 cameraPos)
        {
            Vector3 toTarget = targetPos - cameraPos; toTarget.y = 0;
            Vector3 toPlayer = playerPos - cameraPos; toPlayer.y = 0;
            return Vector3.Angle(toTarget, toPlayer);
        }

        // y(높이)를 무시한 수평 거리.
        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0; b.y = 0;
            return Vector3.Distance(a, b);
        }
    }
}
