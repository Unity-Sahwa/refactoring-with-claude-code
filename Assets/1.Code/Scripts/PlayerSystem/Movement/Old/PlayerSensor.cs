using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 대원_Insite: 
    //  높은 속도로 이동(Rigidbody.MovePosition은 사실 transform 이동)시, 캐릭터가 지형을 뚫고 지나감. 
    //  이를 해결하고자, 이동 전에 충돌 체크를 한 후에 이동하는 Sensor를 구현. 
    //  AI를 통해 충돌을 방지하는 코드를 작성. 하지만 여러 상황에서 지형이 뚫림. 
    //  이럴 때는 사용자가 문제를 이해하고 문제 원인을 짚어주고 해결책을 제시하는 게 좋을 것 같다.

    // 대원_Problem:
    // 현재 코드 이해: 

    // 책임: 이동 컴포넌트가 넘긴 이동량만큼, 플레이어의 실제 CapsuleCollider 모양으로 캡슐을 휩쓸어(CapsuleCast) 충돌을 검사한다.
    //  - 캐스트 캡슐은 콜라이더 치수를 그대로 쓰되 스킨만큼 작게(콜라이더보다 크면 안 됨).
    //  - 검사 출발점을 이동 반대로 백오프해 출발 시 겹침으로 충돌을 놓치는 문제를 줄인다.
    //  - 충돌하면 닿기 직전까지만 전진하고 남은 이동량을 충돌면에 흘린다. 면 법선(뚫는) 방향만 제거하므로
    //    위아래 이동은 살아남고(벽 타고 오르기 가능) 지형 관통은 막힌다.
    //
    // 대원_TODO: 현재 알려진 미해결 문제 모음 (센서 전담 구조는 시도했다가 되돌림. 다시 손댈 때 참고)
    //  [구조 간섭(가장 근본)] 캐릭터가 일반 Rigidbody(중력 켜짐) + 수동 이동(MovePosition+이 센서)을 동시에 굴려 서로 간섭한다.
    //     - 빠른 이동은 RB 콜라이더(CollisionDetection=Discrete)가 못 막아 뚫리고, 코너에선 콜라이더가 끼인다.
    //     - 해결하려면 충돌 책임을 한쪽으로 통일해야 한다: (A) RB를 kinematic으로 두고 이동·낙하·충돌을 전부 코드로,
    //       또는 (B) MovePosition·센서를 버리고 velocity/AddForce + Continuous 충돌로 물리 엔진에 맡김.
    //       (A)를 시도하다 "이동 합산(수평+낙하+스킬을 한 곳에서 MovePosition)·낙하 직접 구현"까지 갔으나 평지 버벅임으로 보류함.
    //  [공중 걷기] 절벽의 90도 안쪽 코너에서 콜라이더가 두 벽에 끼이고 센서가 수평 이동을 0으로 만들어 제자리 고착 →
    //     MovePosition이 매 프레임 같은 y를 덮어써 중력 낙하가 무시됨 → 공중에서 걷는 판정. (일반 낭떠러지는 정상적으로 떨어짐)
    //  [코너 관통] 90도 코너에서 한 벽에 투영한 결과가 옆 벽을 향하는데, 결과를 재검사하지 않아(캐스트 1회) 옆 벽을 파고든다.
    //     → collide & slide 반복(iteration 2~3회) 필요. 비용은 충돌이 있을 때만 늘어난다.
    //  [깊은 시작 겹침] 이미 _backOff보다 깊이 박힌 채 출발하면 CapsuleCast가 그 면을 시작 겹침으로 무시 → 통과.
    //     백오프는 완화일 뿐. Physics.ComputePenetration(겹침 해소: 박힌 만큼 밀어내기)을 병행해야 완전하다.
    //  [박힘 해소 없음] 한번 지형에 박히면(이동량 0 포함) 빠져나오는(밀어내는) 로직이 없어 박힌 채 고정될 수 있다.
    //  [버벅임] 센서 전담(kinematic)으로 갈 경우 MovePosition이 물리 스텝(기본 50Hz)에만 위치를 바꿔 보간 없으면 끊겨 보인다.
    //     → Rigidbody Interpolate=Interpolate 필요, 카메라 추적 타이밍(LateUpdate)도 함께 점검.
    public class PlayerSensor : MonoBehaviour, IPlayerSensor
    {
        [SerializeField] private LayerMask _collisionLayer;     // 막을 대상(벽·지면 등)
        [SerializeField] private float _skinWidth = 0.1f;      // 콜라이더보다 작게 줄이고, 멈출 때 떨어뜨릴 여유
        [SerializeField] private float _backOff = 0.3f;         // 검사 출발점을 이동 반대로 뒤로 뺄 거리(시작 겹침 완화)
        [SerializeField] private float _overlapEpsilon = 0.001f;// 충돌 거리가 이 값 이하면 시작 겹침으로 보고 보수적으로 막음
        [SerializeField] private bool _debugDraw = true;        // 충돌 위치 디버그 표시 on/off

        private readonly List<(Vector3 point, Vector3 normal)> _hitRecords = new();

        // 디버그 스냅샷(마지막 1회)
        private bool _dbgHas;
        private Vector3 _dbgStartP0, _dbgStartP1;   // 실제 출발 캡슐 끝점
        private Vector3 _dbgCastP0, _dbgCastP1;     // 백오프한 캐스트 시작 캡슐 끝점
        private float _dbgRadius;                   // 캐스트 캡슐 반지름
        private Vector3 _dbgDir;
        private float _dbgCastDist;
        private bool _dbgHit;
        private float _dbgHitDist;
        private Vector3 _dbgResult;

        public Vector3 ResolveMove(Vector3 currentPos, Vector3 intendedDelta, CapsuleCollider collider)
        {
            //1차적으로 캐릭터 콜라이더가 지형을 뚫는 것을 방지
            //2차적으로 이 메서드를 통해서 겹치는 부분, 높은 속도의 이동으로 뚫리는 것을 사전에 방지
            //

            float distance = intendedDelta.magnitude;
            if (distance <= 0f || collider == null)
            {
                _dbgHas = false;
                return intendedDelta;
            }

            Vector3 dir = intendedDelta / distance;

            // 콜라이더의 월드 캡슐(두 끝점·반지름)을 구하고, currentPos와 콜라이더 위치 차이를 보정한다.
            GetWorldCapsule(collider, out Vector3 p0, out Vector3 p1, out float radius);
            Vector3 posFix = currentPos - collider.transform.position;
            p0 += posFix;
            p1 += posFix;

            // 캐스트 반지름은 콜라이더보다 스킨만큼 작게(콜라이더가 1차로 닿고, 캐스트는 그 안쪽).
            float castRadius = Mathf.Max(0.01f, radius - _skinWidth);

            // 백오프: 캐스트 시작 캡슐을 이동 반대로 뒤로 빼고, 검사 거리는 그만큼 늘린다.
            Vector3 back = dir * _backOff;
            Vector3 castP0 = p0 - back;
            Vector3 castP1 = p1 - back;
            float castDistance = _backOff + distance + _skinWidth;

            // 디버그 스냅샷
            _dbgHas = true;
            _dbgStartP0 = p0;
            _dbgStartP1 = p1;
            _dbgCastP0 = castP0;
            _dbgCastP1 = castP1;
            _dbgRadius = castRadius;
            _dbgDir = dir;
            _dbgCastDist = castDistance;
            _dbgHit = false;

            if (Physics.CapsuleCast(castP0, castP1, castRadius, dir, out RaycastHit hit,
                                    castDistance, _collisionLayer, QueryTriggerInteraction.Ignore))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                _dbgHit = true;
                _dbgHitDist = hit.distance;

                if (_debugDraw)
                {
                    _hitRecords.Add((hit.point, hit.normal));
                    Debug.Log($"[PlayerSensor] 충돌 point={hit.point} normal={hit.normal} angle={slopeAngle:F1} hitDist={hit.distance:F3} dot={Vector3.Dot(hit.normal, dir):F2} collider={hit.collider.name}");
                }

                Vector3 result;
                if (hit.distance <= _overlapEpsilon)
                {
                    // 시작 겹침: 법선을 믿을 수 없으므로 보수적으로 이 방향 이동을 막는다.
                    result = Vector3.zero;
                }
                else if (Vector3.Dot(hit.normal, dir) > 0f)
                {
                    // 등 뒤 면(백오프 탓에 잡힌, 이동을 가로막지 않는 면): 무시하고 그대로 이동.
                    result = intendedDelta;
                }
                else
                {
                    // 앞 면: 닿기 직전까지 전진(백오프·스킨 환산) 후, 남은 이동을 면을 따라 흘린다(투영).
                    // 면 법선(뚫는) 방향 성분만 제거되므로 위아래는 살고 지형 관통은 막힌다.
                    float allowed = Mathf.Max(0f, hit.distance - _backOff - _skinWidth);
                    Vector3 moved = dir * allowed;
                    result = moved + Vector3.ProjectOnPlane(intendedDelta - moved, hit.normal);
                }

                _dbgResult = result;
                return result;
            }

            _dbgResult = intendedDelta;
            return intendedDelta;
        }

        // CapsuleCollider를 월드 기준 두 끝점과 반지름으로 변환한다(스케일·회전·축 방향 반영).
        private void GetWorldCapsule(CapsuleCollider capsule, out Vector3 p0, out Vector3 p1, out float radius)
        {
            Transform t = capsule.transform;
            Vector3 scale = t.lossyScale;

            Vector3 axis;
            float heightScale, radiusScale;
            switch (capsule.direction)
            {
                case 0: // X축
                    axis = t.right;
                    heightScale = Mathf.Abs(scale.x);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                    break;
                case 2: // Z축
                    axis = t.forward;
                    heightScale = Mathf.Abs(scale.z);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
                    break;
                default: // 1: Y축
                    axis = t.up;
                    heightScale = Mathf.Abs(scale.y);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                    break;
            }

            radius = capsule.radius * radiusScale;
            float height = Mathf.Max(capsule.height * heightScale, radius * 2f);
            float half = height * 0.5f - radius;

            Vector3 center = t.TransformPoint(capsule.center);
            p0 = center + axis * half;
            p1 = center - axis * half;
        }

        // 캡슐 모양을 와이어로 그린다(양 끝 구 + 옆면 선 4개). 축은 두 끝점에서 구한다(기운 캡슐도 대응).
        private void DrawWireCapsule(Vector3 p0, Vector3 p1, float radius)
        {
            Gizmos.DrawWireSphere(p0, radius);
            Gizmos.DrawWireSphere(p1, radius);

            Vector3 axis = p0 - p1;
            axis = axis.sqrMagnitude < 1e-6f ? Vector3.up : axis.normalized;
            Vector3 side1 = Vector3.Cross(axis, Vector3.up);
            if (side1.sqrMagnitude < 1e-6f)
            {
                side1 = Vector3.Cross(axis, Vector3.forward);
            }
            side1.Normalize();
            Vector3 side2 = Vector3.Cross(axis, side1).normalized;

            Gizmos.DrawLine(p0 + side1 * radius, p1 + side1 * radius);
            Gizmos.DrawLine(p0 - side1 * radius, p1 - side1 * radius);
            Gizmos.DrawLine(p0 + side2 * radius, p1 + side2 * radius);
            Gizmos.DrawLine(p0 - side2 * radius, p1 - side2 * radius);
        }

        // Scene 뷰 디버그.
        //  흰색=실제 출발 / 자홍=백오프 캐스트 시작(겹치면 안 됨) / 회색=검사 끝
        //  노란선=경로 / 빨강=캐스트 멈춘 위치(안 보이면 인식 실패) / 파랑=실제 멈추는 위치 / 초록=충돌 없이 도달
        //  노란구=누적 충돌점 / 파란선=법선
        private void OnDrawGizmos()
        {
            if (!_debugDraw)
            {
                return;
            }

            if (_dbgHas)
            {
                Gizmos.color = Color.white;
                DrawWireCapsule(_dbgStartP0, _dbgStartP1, _dbgRadius);

                Gizmos.color = Color.magenta;
                DrawWireCapsule(_dbgCastP0, _dbgCastP1, _dbgRadius);

                Vector3 endOffset = _dbgDir * _dbgCastDist;
                Gizmos.color = Color.gray;
                DrawWireCapsule(_dbgCastP0 + endOffset, _dbgCastP1 + endOffset, _dbgRadius);

                Vector3 castCenter = (_dbgCastP0 + _dbgCastP1) * 0.5f;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(castCenter, castCenter + endOffset);

                if (_dbgHit)
                {
                    Vector3 hitOffset = _dbgDir * _dbgHitDist;
                    Gizmos.color = Color.red;
                    DrawWireCapsule(_dbgCastP0 + hitOffset, _dbgCastP1 + hitOffset, _dbgRadius);

                    Gizmos.color = Color.blue;
                    DrawWireCapsule(_dbgStartP0 + _dbgResult, _dbgStartP1 + _dbgResult, _dbgRadius);
                }
                else
                {
                    Gizmos.color = Color.green;
                    DrawWireCapsule(_dbgStartP0 + _dbgResult, _dbgStartP1 + _dbgResult, _dbgRadius);
                }
            }

            foreach (var record in _hitRecords)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(record.point, 0.05f);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(record.point, record.point + record.normal * 0.5f);
            }
        }
    }
}
