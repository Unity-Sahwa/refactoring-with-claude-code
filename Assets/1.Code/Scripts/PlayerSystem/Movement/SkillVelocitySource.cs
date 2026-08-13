using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Refactoring
{
    // 역할: 상태시스템에서 주는 스킬 이동 데이터를 합산된 속도로 만들어 Mover에게 전달한다.
    // 왜 존재: 원래는 PlayerSkillMoveHandler의 로직으로 캐릭터를 이동시키는 로직이 포함되었으나, CharacterController.Move로 이동방식을 변경하고, 이동을 한 곳으로 모으기 위해 이동 소스로서 존재
    public class SkillVelocitySource : IVelocitySource
    {
        private readonly IPlayerStateEventSubscriber _subscriber;
        private readonly List<ActiveSkillMove> _actives = new();
        private readonly IDisposable _skillMoveEventDisposable;

        // ponytail: 레이어명·버퍼크기 하드코딩. 탐지 대상이 늘면 그때 설정으로 뺀다.
        private static readonly int EnemyMask = LayerMask.GetMask("Enemy");
        private static readonly Collider[] OverlapBuffer = new Collider[8];
        private const float MarkerLifetime = 1f; // 검사 캡슐 마커가 남아 있는 시간
        private const float AirborneEpsilon = 0.05f; // 지면 요철·부동소수 오차로 떠 있다고 오판하지 않을 여유
        // ponytail: 탐지 박스 치수 하드코딩. 스킬별로 달라져야 하면 ISkillMove에 넣는다.
        private const float BoxWidth = 0.5f;   // 좌우
        private const float BoxDepth = 0.5f; // 앞뒤
        private const float BoxForwardOffset = 1;

        // 스킬 시작 순간의 발 높이. 스킬당 한 번만 기록한다(조각마다 잡으면 이미 뜬 뒤의 공중 높이가 저장됨).
        private float _skillStartY = float.NaN;

        private class ActiveSkillMove
        {
            public Vector3 localVelocity; // Direction.normalized * Speed (캐릭터 로컬 기준, 미리 계산)
            public float duration;
            public float elapsed;
        }

        public SkillVelocitySource(IPlayerStateEventSubscriber subscriber)
        {
            _subscriber = subscriber;

            if (_subscriber != null)
            {
                _skillMoveEventDisposable = _subscriber.Register(StateEventCategory.SkillMove, HandleSkillMove, HandleReset);
            }
        }

        public Vector3 Evaluate(in MoveParams frame)
        {
            if (_actives.Count == 0)
            {
                return Vector3.zero;
            }

            if (float.IsNaN(_skillStartY))
            {
                _skillStartY = frame.CharacterTransform.position.y;
            }

            // 역순 순회로 만료된 이동을 제거(삭제해도 남은 인덱스에 영향 없음).
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                _actives[i].elapsed += frame.DeltaTime;
                if (_actives[i].elapsed >= _actives[i].duration)
                {
                    _actives.RemoveAt(i);
                }
            }

            if (_actives.Count == 0)
            {
                _skillStartY = float.NaN; // 시간 만료로 다 빠진 경우도 초기화
                return Vector3.zero;
            }

            // localVelocity를 캐릭터가 바라보는 방향으로 돌려서 모두 더한다.
            // 동시에 위로 향하는 조각이 있는지 본다(합산 y로 보면 하강 조각과 상쇄돼 놓친다).
            Vector3 worldVelocity = Vector3.zero;
            bool hasRise = false;
            for (int i = 0; i < _actives.Count; i++)
            {
                worldVelocity += frame.CharacterTransform.rotation * _actives[i].localVelocity;
                if (_actives[i].localVelocity.y > 0f)
                {
                    hasRise = true;
                }
            }

            // 상승 조각이 살아있는 동안 + 시작 높이보다 떠 있는 동안 검사한다.
            // (상승이 끝나고 앞으로만 가는 구간에서 적 머리 위를 지나치는 걸 막으려면 후자가 필요하다)
            // 앞으로 나아갈 때만 검사한다(백덤블링처럼 뒤로 가는 이동은 앞쪽 박스에 걸릴 이유가 없다).
            bool movingForward = (Quaternion.Inverse(frame.CharacterTransform.rotation) * worldVelocity).z > 0f;
            bool airborne = frame.CharacterTransform.position.y > _skillStartY + AirborneEpsilon;
            if (movingForward && (hasRise || airborne) && OverlapEnemyInFront(frame, _skillStartY))
            {
                // 수평만 죽인다. y까지 죽이면 공중에 그대로 멈춰버린다(상승·낙하는 그대로 진행돼야 함).
                worldVelocity.x = 0f;
                worldVelocity.z = 0f;
            }
            return worldVelocity;
        }

        // 플레이어 앞(진행 방향 쪽)에 적이 있는지 겹침 검사한다.
        // 박스는 시작 높이~현재 높이 구간을 덮는다: 현재 높이만 보면 뜬 뒤 적 머리 위를 지나쳐 놓친다.
        // 캡슐이 아닌 박스인 이유: 반지름 하나로는 좌우만 넓히고 앞뒤는 얇게 두는 게 안 된다.
        private bool OverlapEnemyInFront(in MoveParams frame, float startY)
        {
            CharacterController controller = frame.Controller;
            Transform character = controller.transform;
            Vector3 center = character.TransformPoint(controller.center);

            float height = Mathf.Max(center.y - startY, 0.01f); // 시작 발높이 ~ 현재 캡슐 중심
            Vector3 boxCenter = new Vector3(center.x, startY + height * 0.5f, center.z)
                                + character.forward * BoxForwardOffset;
            Vector3 halfExtents = new Vector3(BoxWidth * 0.5f, height * 0.5f, BoxDepth * 0.5f);

            int count = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, OverlapBuffer,
                character.rotation, EnemyMask, QueryTriggerInteraction.Ignore);

            //Debug 용
            //GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //UnityEngine.Object.Destroy(marker.GetComponent<Collider>());
            //marker.transform.SetPositionAndRotation(boxCenter, character.rotation);
            //marker.transform.localScale = halfExtents * 2f; // 큐브는 기본 한 변 1
            //UnityEngine.Object.Destroy(marker, MarkerLifetime);

            return count > 0;
        }
        
        public void OnCharacterChanged() => ClearActives();

        public void Dispose()
        {
            _skillMoveEventDisposable?.Dispose();
            _actives.Clear();
        }

        private void ClearActives()
        {
            _actives.Clear();
            _skillStartY = float.NaN; // 다음 스킬에서 시작 높이를 새로 잡게 초기화
        }

        // 상태가 스킬 이동을 켤 때 호출. ISkillMove 데이터(방향·속도·시간) 그대로 활성 목록에 추가한다.
        private void HandleSkillMove(IStartData data)
        {
            if (data is not ISkillMove skillMoveData)
            {
                Debug.LogError($"[SkillVelocitySource] ISkillMove가 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            _actives.Add(new ActiveSkillMove
            {
                localVelocity = skillMoveData.Direction.normalized * skillMoveData.Speed,
                duration = skillMoveData.Duration,
                elapsed = 0f
            });
        }

        // 상태 전환 시 진행 중인 스킬 이동을 모두 끈다(다음 상태로 새지 않게). SkillMove엔 End가 없어 reason은 항상 Reset.
        private void HandleReset(CloseEventType reason) => ClearActives();
    }
}
