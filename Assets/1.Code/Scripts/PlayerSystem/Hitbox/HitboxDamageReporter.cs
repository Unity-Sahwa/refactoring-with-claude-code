using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 히트박스 프리팹에 붙어, 켜진 동안 매 물리 스텝 Overlap으로 겹친 대상에게 전투 정보를 전달한다.
    // 전투값과 공격자는 핸들러가 켤 때 Setup으로 주입한다([SerializeField]로 직접 받지 않는다).
    // 모양은 자식의 BoxCollider/CapsuleCollider/SphereCollider로 정의하며(isTrigger 권장), 같은 대상은 _alreadyHit으로 한 번만 타격한다.

    //Project Settings > Tags and Layers : Player, Enemy, Gimmick 레이어 추가

    public class HitboxDamageReporter : MonoBehaviour, IHitboxDamageReporter
    {
        private IHitboxCombat _combat;
        private GameObject _damager;
        private Collider[] _colliders;       // 모양 정의용. Box/Capsule 콜라이더(자식 포함)
        private int _targetMask;             // 타격 대상 레이어
        private readonly Collider[] _buffer = new Collider[16];   // Overlap 결과 버퍼(GC 방지)
        private readonly HashSet<IDamageable> _alreadyHit = new(); // 한 번 켜진 동안 중복 타격 방지

        private void Awake()
        {
            _colliders = GetComponentsInChildren<Collider>(true); // 자식 포함, 비활성도 수집
        }

        // 대상 레이어(targetMask)는 공격 주체(핸들러)가 정해 넘긴다 → Reporter는 적·플레이어 공용.
        public void Setup(IHitboxCombat combat, GameObject damager, int targetMask)
        {
            _combat = combat;
            _damager = damager;
            _targetMask = targetMask;
        }

        private void OnEnable()
        {
            _alreadyHit.Clear(); // 켜질 때마다 초기화해, 다음 휘두름에서 다시 맞을 수 있게 한다.
        }

        // 켜진 동안 매 물리 스텝마다 각 콜라이더 영역을 검사한다(켜질 때 내부에 있던 대상도 잡힌다).
        private void FixedUpdate()
        {
            if (_combat == null) return;

            foreach (var col in _colliders)
            {
                if (col == null) continue;

                int count = OverlapCollider(col, _buffer);
                Vector3 center = col.bounds.center;
                for (int i = 0; i < count; i++)
                {
                    TryHit(_buffer[i], center);
                }
            }
        }

        // 콜라이더 종류별로 알맞은 Overlap 검사를 한다. Box/Capsule/Sphere만 지원한다.
        private int OverlapCollider(Collider col, Collider[] results)
        {
            switch (col)
            {
                case BoxCollider box:
                {
                    Vector3 center = box.transform.TransformPoint(box.center);
                    Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale);
                    return Physics.OverlapBoxNonAlloc(center, halfExtents, results, box.transform.rotation, _targetMask);
                }
                case CapsuleCollider capsule:
                {
                    GetCapsuleWorld(capsule, out var p0, out var p1, out var radius);
                    return Physics.OverlapCapsuleNonAlloc(p0, p1, radius, results, _targetMask);
                }
                case SphereCollider sphere:
                {
                    Vector3 center = sphere.transform.TransformPoint(sphere.center);
                    float radius = sphere.radius * MaxAbsScale(sphere.transform.lossyScale);
                    return Physics.OverlapSphereNonAlloc(center, radius, results, _targetMask);
                }
                default:
                    return 0; // Box/Capsule/Sphere 외 모양은 지원하지 않음
            }
        }

        // 비균등 스케일에서 구의 반지름은 세 축 중 가장 큰 스케일을 따른다(Unity SphereCollider와 동일).
        private static float MaxAbsScale(Vector3 s) =>
            Mathf.Max(Mathf.Abs(s.x), Mathf.Max(Mathf.Abs(s.y), Mathf.Abs(s.z)));

        // CapsuleCollider를 월드 기준 두 끝점과 반지름으로 변환한다(스케일·회전·축 방향 반영).
        private void GetCapsuleWorld(CapsuleCollider capsule, out Vector3 p0, out Vector3 p1, out float radius)
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

        private void TryHit(Collider hitCollider, Vector3 colliderCenter)
        {
            // 자식 콜라이더에 맞아도 Rigidbody가 있는 본체의 IDamageable을 찾는다.
            var body = hitCollider.attachedRigidbody;
            var target = body != null
                ? body.GetComponent<IDamageable>()
                : hitCollider.GetComponentInParent<IDamageable>();

            if (target == null) return;
            if (!_alreadyHit.Add(target)) return; // 이미 맞은 대상이면 무시

            var info = new DamageInfo
            {
                Damager = _damager,
                Amount = _combat.Damage,
                HitPoint = hitCollider.ClosestPoint(colliderCenter), // 콜라이더 중심 기준 표면점 근사
                Color = _combat.Color,
                InkStack = _combat.InkStack,
                Reactions = _combat.Reactions
            };

            target.ApplyDamage(info);
        }

        // 검사에 실제로 쓰는 영역을 와이어로 그려, 콜라이더 모양과 맞는지 확인한다.
        private void OnDrawGizmos()
        {
            var colliders = _colliders ?? GetComponentsInChildren<Collider>(true);
            Gizmos.color = Color.blue;

            foreach (var col in colliders)
            {
                if (col == null) continue;

                switch (col)
                {
                    case BoxCollider box:
                        Gizmos.matrix = Matrix4x4.TRS(
                            box.transform.TransformPoint(box.center),
                            box.transform.rotation,
                            box.transform.lossyScale);
                        Gizmos.DrawWireCube(Vector3.zero, box.size);
                        Gizmos.matrix = Matrix4x4.identity;
                        break;

                    case CapsuleCollider capsule:
                        GetCapsuleWorld(capsule, out var p0, out var p1, out var radius);
                        Gizmos.DrawWireSphere(p0, radius);
                        Gizmos.DrawWireSphere(p1, radius);
                        break;

                    case SphereCollider sphere:
                        Gizmos.DrawWireSphere(
                            sphere.transform.TransformPoint(sphere.center),
                            sphere.radius * MaxAbsScale(sphere.transform.lossyScale));
                        break;
                }
            }
        }
    }
}
