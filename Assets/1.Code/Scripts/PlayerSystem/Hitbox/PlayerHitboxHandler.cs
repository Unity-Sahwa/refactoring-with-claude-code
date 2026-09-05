using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 상태 이벤트로 히트박스를 켜고, 겹친 대상에게 데미지를 주고 타격 성공을 발행한다.
    // 흐름: 이벤트로 활성 목록 추가 → FixedUpdate마다 겹침 검사 → 대상에 ApplyDamage → HitChannel 발행
    public class PlayerHitboxHandler : MonoBehaviour
    {
        [Preserve, Inject] private IPlayerStateEventSubscriber _eventSubscriber;
        [Preserve, Inject] private ICurrentCharacterProvider _currentCharacterProvider;
        [Preserve, Inject] private HitChannel _hitChannel;

        private int _targetMask;
        private readonly List<ActiveHitbox> _actives = new();

        // 겹침 결과 버퍼(GC 방지). 넘치면 뒤쪽은 잘린다.
        private readonly Collider[] _buffer = new Collider[32];
        private IDisposable _hitboxEventDisposable;

        // 활성화된 히트박스에 대한 정보
        private class ActiveHitbox
        {
            public IPlayerHitbox Data;
            public Transform Attach;
            public float EndTime;
            public readonly HashSet<IDamageable> AlreadyHit = new();
        }

        private void Awake()
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

            Transform attacker = _currentCharacterProvider?.GetCurrentComponent<Transform>();
            if (attacker == null)
            {
                return;
            }

            _actives.Add(new ActiveHitbox
            {
                Data = hitbox,
                Attach = attacker,
                EndTime = Time.time + hitbox.Duration
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

                if (Time.time >= active.EndTime)
                {
                    _actives.RemoveAt(i);
                    continue;
                }

                // 캐릭터 정면 기준 히트박스의 월드 좌표
                Vector3 position = active.Attach.TransformPoint(active.Data.Position);
                Quaternion rotation = active.Attach.rotation * Quaternion.Euler(active.Data.Rotation);

                int count = Overlap(active.Data, position, rotation);
                for (int j = 0; j < count; j++)
                {
                    TryHit(active, _buffer[j], position);
                }
            }
        }

        private int Overlap(IPlayerHitbox data, Vector3 position, Quaternion rotation)
        {
            Vector3 size = data.ShapeScale;

            switch (data.Shape)
            {
                case HitboxShape.Sphere:
                    return Physics.OverlapSphereNonAlloc(position, size.x * 0.5f, _buffer, _targetMask);

                case HitboxShape.Capsule:
                    GetCapsuleEnds(size, position, rotation, out Vector3 topPoint, out Vector3 bottomPoint, out float radius);
                    return Physics.OverlapCapsuleNonAlloc(topPoint, bottomPoint, radius, _buffer, _targetMask);

                default:
                    return Physics.OverlapBoxNonAlloc(position, size * 0.5f, _buffer, rotation, _targetMask);
            }
        }

        // 크기(size)를 유니티가 요구하는 두 끝점(양 끝 반구의 중심)과 반지름으로 바꾼다.
        private void GetCapsuleEnds(Vector3 size, Vector3 position, Quaternion rotation, out Vector3 topPoint, out Vector3 bottomPoint, out float radius)
        {
            radius = size.x * 0.5f;
            // 높이가 지름보다 작아지는 문제 방지
            float half = Mathf.Max(size.y, radius * 2f) * 0.5f - radius;
            // Quaternion * Vector3는 그 방향을 회전시킨 결과라 캡슐의 실제 축이 나온다.
            Vector3 axis = rotation * Vector3.up;
            topPoint = position + axis * half;
            bottomPoint = position - axis * half;
        }

        private void TryHit(ActiveHitbox active, Collider hitCollider, Vector3 center)
        {
            IDamageable target = hitCollider.GetComponent<IDamageable>();
            // GetComponent 실패이거나, 참조만 남고 실제 오브젝트는 삭제된 경우를 함께 거른다.
            if (target is not Component component || component == null)
            {
                return;
            }

            if (!active.AlreadyHit.Add(target))
            {
                return;
            }

            CombatInfo combat = active.Data.Combat;
            DamageInfo info = new DamageInfo
            {
                Damager = active.Attach.gameObject,
                Amount = combat.Damage,
                HitPoint = hitCollider.ClosestPoint(center),
                Color = combat.Color,
                InkStack = combat.InkStack
            };

            target.ApplyDamage(info);

            // 타격 성공 사실만 발행한다. 소리·히트스탑 등은 구독자가 알아서 처리한다.
            if (_hitChannel != null)
            {
                _hitChannel.Raise(new HitReport
                {
                    Attacker = active.Attach.gameObject,
                    Target = component.gameObject,
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
                Vector3 position = active.Attach.TransformPoint(active.Data.Position);
                Quaternion rotation = active.Attach.rotation * Quaternion.Euler(active.Data.Rotation);
                Vector3 size = active.Data.ShapeScale;

                switch (active.Data.Shape)
                {
                    case HitboxShape.Sphere:
                        Gizmos.DrawWireSphere(position, size.x * 0.5f);
                        break;
                    case HitboxShape.Capsule:
                        GetCapsuleEnds(size, position, rotation, out Vector3 topPoint, out Vector3 bottomPoint, out float radius);
                        Gizmos.DrawWireSphere(topPoint, radius);
                        Gizmos.DrawWireSphere(bottomPoint, radius);
                        break;
                    default:
                        Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
                        Gizmos.DrawWireCube(Vector3.zero, size);
                        Gizmos.matrix = Matrix4x4.identity;
                        break;
                }
            }
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}