using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 상태 이벤트의 알림과 함께 전달받은 데이터(위치, 시간범위 등)로 IHitboxProvider으로부터 제공된 히트박스를 활성화시킨다.
    public class PlayerHitboxHandler : MonoBehaviour
    {
        [Inject] private IPlayerStateEventSubscriber _eventSubscriber;
        [Inject] private IHitboxProvider _provider;
        [Inject] private List<IHitboxAttachPoint> _attachPoints;
        private int _targetMask;   // 플레이어 공격이 때릴 대상 레이어. 적 핸들러가 생기면 abstract로 분리한다.
        private readonly Dictionary<HitboxAttachPointType, Transform> _attachPointMap = new();
        private readonly List<ActiveHitbox> _actives = new();

        private class ActiveHitbox
        {
            public GameObject instance;
            public Coroutine routine;
            public bool untilFinish;
        }

        void Awake()
        {
            _eventSubscriber.Subscribe(StateEventCategory.Hitbox, HandleHitbox);
            _eventSubscriber.SubscribeReset(HandleReset);

            foreach (var point in _attachPoints)
            {
                _attachPointMap[point.Key] = point.Transform;
            }

            _targetMask = LayerMask.GetMask("Enemy", "Gimmick");
        }

        //대원_TODO: 원하는 타이밍에 이벤트 호출되는게 아니라 원하는 타이밍보다 늦을 수 있음. 필요시 Animation Event로 타이밍을 2중 감시하기
        private void HandleHitbox(PlayerCharacter source, IStartData data)
        {
            var hitbox = (IPlayerHitbox)data;

            var instance = _provider.Rent(hitbox.HitboxObject);
            if (instance == null) return;

            if (!_attachPointMap.TryGetValue(hitbox.AttachKey, out var parent))
            {
                _provider.Return(instance);
                return;
            }

            var t = instance.transform;
            t.SetParent(parent, false);
            t.localPosition = hitbox.Position;
            t.localRotation = Quaternion.Euler(hitbox.Rotation);
            if (hitbox.Scale == Vector3.zero) t.localScale = Vector3.one;
            else t.localScale = hitbox.Scale;

            _provider.SetMeshVisible(instance, hitbox.ShowMesh);

            // 전투값과 공격자(이벤트를 발행한 캐릭터)를 감지 컴포넌트에 주입한다.
            var reporter = _provider.GetReporter(instance);
            if (reporter != null)
            {
                reporter.Setup((IHitboxCombat)data, source != null ? source.gameObject : null, _targetMask);
            }

            instance.SetActive(true);

            var active = new ActiveHitbox { instance = instance, untilFinish = hitbox.UntilFinish };
            active.routine = StartCoroutine(CoRunHitbox(active, hitbox));
            _actives.Add(active);
        }

        private IEnumerator CoRunHitbox(ActiveHitbox active, IPlayerHitbox hitbox)
        {
            if (hitbox.StopInPlace)
            {
                float stopTime = Mathf.Clamp(hitbox.StopTime, 0f, hitbox.Duration);
                yield return new WaitForSeconds(stopTime);
                active.instance.transform.SetParent(null, true);
                yield return new WaitForSeconds(hitbox.Duration - stopTime);
            }
            else
            {
                yield return new WaitForSeconds(hitbox.Duration);
            }

            FinishHitbox(active);
        }

        // 왜: untilFinish 히트박스는 상태가 끝나도 살아남아야 한다. 다만 캐릭터 자식으로 붙어 있으면
        //     이후 스왑(캐릭터 비활성화) 시 같이 꺼지므로, 이 시점에 부모에서 분리해 월드로 독립시킨다.
        private void HandleReset()
        {
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                var active = _actives[i];
                if (active.untilFinish)
                {
                    active.instance.transform.SetParent(null, true);
                    continue;
                }

                if (active.routine != null) StopCoroutine(active.routine);
                active.instance.SetActive(false);
                FinishHitbox(active);
            }
        }

        private void FinishHitbox(ActiveHitbox active)
        {
            _actives.Remove(active);
            _provider.Return(active.instance);
        }

        private void OnDestroy()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe(StateEventCategory.Hitbox, HandleHitbox);
                _eventSubscriber.UnsubscribeReset(HandleReset);
            }

            foreach (var active in _actives)
            {
                if (active.routine != null) StopCoroutine(active.routine);
            }
            _actives.Clear();
        }
    }
}
