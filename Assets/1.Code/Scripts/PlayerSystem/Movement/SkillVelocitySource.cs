using System;
using System.Collections.Generic;
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

            // 역순 순회로 만료된 이동을 제거(삭제해도 남은 인덱스에 영향 없음).
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                _actives[i].elapsed += frame.DeltaTime;
                if (_actives[i].elapsed >= _actives[i].duration)
                {
                    _actives.RemoveAt(i);
                }
            }

            // localVelocity를 캐릭터가 바라보는 방향으로 돌려서 모두 더한다.
            Vector3 worldVelocity = Vector3.zero;
            for (int i = 0; i < _actives.Count; i++)
            {
                worldVelocity += frame.CharacterTransform.rotation * _actives[i].localVelocity;
            }
            return worldVelocity;
        }

        public void OnCharacterChanged() => _actives.Clear();

        public void Dispose()
        {
            _skillMoveEventDisposable?.Dispose();
            _actives.Clear();
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
        private void HandleReset(CloseEventType reason) => _actives.Clear();
    }
}
