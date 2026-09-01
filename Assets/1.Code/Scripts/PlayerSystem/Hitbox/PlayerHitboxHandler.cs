using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 역할: 플레이어 상태 이벤트를 알림 받아, overlap 형태로 충돌체 활성화. 충돌된 대상에게 데이터 주입 및 충돌 성공 이벤트 호출
    public class PlayerHitboxHandler : MonoBehaviour
    {
        [Preserve, Inject] private IPlayerStateEventSubscriber _eventSubscriber;
        [Preserve, Inject] private ICurrentCharacterProvider _currentCharacterProvider;
        [Preserve, Inject] private HitChannel hitChannel;

        private int _targetMask;
        private readonly List<ActiveHitbox> _actives = new();
        private readonly Collider[] _buffer = new Collider[32];   // 겹침 결과 버퍼(GC 방지). 넘치면 뒤쪽은 잘림
        private IDisposable _hitboxEventDisposable;

        // 활성화된 히트박스에 대한 정보
        private class ActiveHitbox
        {
            public IPlayerHitbox data;
            public Transform attach;
            public float endTime;
            public readonly HashSet<IDamageable> alreadyHit = new();
        }

        void Awake()
        {
            _hitboxEventDisposable = _eventSubscriber.Register(StateEventCategory.Hitbox, HandleHitbox, HandleReset);

            _targetMask = LayerMask.GetMask("Enemy", "Gimmick");
            if (_targetMask == 0) 
            {
                Debug.LogError("[PlayerHitboxHandler] targetMask가 0임. 아무도 안 맞음");
            }
        }

        private void HandleHitbox(IStartData data)
        {
            if (data is not IPlayerHitbox hitbox)
            {
                Debug.LogError($"[PlayerHitboxHandler] IPlayerHitbox가 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            PlayerCharacter attacker = _currentCharacterProvider?.CurrentCharacter;
            if (attacker == null) 
            {
                return;
            }

            _actives.Add(new ActiveHitbox
            {
                data = hitbox,
                attach = attacker.transform,
                endTime = Time.time + hitbox.Duration
            });
        }

        private void HandleReset(CloseEventType reason)
        {
            _actives.Clear();
        }

        // 켜져 있는 동안 매 물리 스텝마다 각 히트박스 영역을 검사한다(켜진 순간 이미 안에 있던 대상도 잡힌다).
        private void FixedUpdate()
        {
            // 시간 다 된 히트박스를 도중에 빼도 인덱스가 안 꼬임.
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                var active = _actives[i];

                if (Time.time >= active.endTime)
                {
                    _actives.RemoveAt(i);
                    continue;
                }

                var pos = active.attach.TransformPoint(active.data.Position); // attach 정면기준 hitbox의 월드좌표 반환
                var rot = active.attach.rotation * Quaternion.Euler(active.data.Rotation);

                int count = Overlap(active.data, pos, rot);
                for (int j = 0; j < count; j++)
                {
                    TryHit(active, _buffer[j], pos);
                }
            }
        }

        private int Overlap(IPlayerHitbox data, Vector3 pos, Quaternion rot)
        {
            Vector3 size = data.ShapeScale;
            
            switch (data.Shape)
            {
                case HitboxShape.Sphere:
                    return Physics.OverlapSphereNonAlloc(pos, size.x * 0.5f, _buffer, _targetMask);

                case HitboxShape.Capsule:
                    GetCapsuleEnds(size, pos, rot, out var p0, out var p1, out var radius);
                    return Physics.OverlapCapsuleNonAlloc(p0, p1, radius, _buffer, _targetMask);

                default:
                    return Physics.OverlapBoxNonAlloc(pos, size * 0.5f, _buffer, rot, _targetMask);
            }
        }

        // 캡슐 양 끝 반구의 중심점 캡슐을 유니티가 요구하는 두 끝점(구 중심)과 반지름으로 바꾼다.
        private void GetCapsuleEnds(Vector3 size, Vector3 pos, Quaternion rot, out Vector3 p0, out Vector3 p1, out float radius)
        {
            radius = size.x * 0.5f;
            float half = Mathf.Max(size.y, radius * 2f) * 0.5f - radius; //높이가 지름보다 작아지는 문제 방지
            Vector3 axis = rot * Vector3.up; // 캡슐의 실제 축을 나타냄. Quaternion * Vector3는 Vector3 화살표를 Quaternion만큼 회전을 의미
            p0 = pos + axis * half;
            p1 = pos - axis * half;
        }

        private void TryHit(ActiveHitbox active, Collider hitCollider, Vector3 center)
        {
            var target = hitCollider.GetComponent<IDamageable>();

            // GetComponent 실패 또는 유니티 오브젝트가 아닌지 || 참조가 가리키는 대상이 삭제되었는지.(참조만 존재하는 문제)
            if (target is not Component componenet || componenet == null) 
            {
                return;
            }
            
            if (!active.alreadyHit.Add(target)) 
            {
                return;
            }
                
            var combat = active.data.Combat;
            var info = new DamageInfo
            {
                Damager = active.attach.gameObject,
                Amount = combat.Damage,
                HitPoint = hitCollider.ClosestPoint(center), // 판정 영역 중심 기준 표면점 근사
                Color = combat.Color,
                InkStack = combat.InkStack
            };

            target.ApplyDamage(info);

            // 타격 성공 사실만 발행한다. 소리·히트스탑 등은 구독자가 알아서 처리한다.
            if (hitChannel != null)
            {
                hitChannel.Raise(new HitReport
                {
                    Attacker = active.attach.gameObject,
                    Target = componenet.gameObject,
                    Point = info.HitPoint,
                    Sound = combat.HitSound
                });
            }
        }
        private void OnDestroy()
        {
            _hitboxEventDisposable?.Dispose();
            _actives.Clear();
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            foreach (var active in _actives)
            {
                var pos = active.attach.TransformPoint(active.data.Position);
                var rot = active.attach.rotation * Quaternion.Euler(active.data.Rotation);

                Vector3 size = active.data.ShapeScale;

                switch (active.data.Shape)
                {
                    case HitboxShape.Sphere:
                        Gizmos.DrawWireSphere(pos, size.x * 0.5f);
                        break;

                    case HitboxShape.Capsule:
                        GetCapsuleEnds(size, pos, rot, out var p0, out var p1, out var radius);
                        Gizmos.DrawWireSphere(p0, radius);
                        Gizmos.DrawWireSphere(p1, radius);
                        break;

                    default:
                        Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);
                        Gizmos.DrawWireCube(Vector3.zero, size);
                        Gizmos.matrix = Matrix4x4.identity;
                        break;
                }
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
