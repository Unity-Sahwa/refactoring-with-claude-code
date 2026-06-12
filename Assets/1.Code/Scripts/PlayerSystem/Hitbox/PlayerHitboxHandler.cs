using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class PlayerHitboxHandler : MonoBehaviour, IInterfaceInjectable, IDataInjectable
    {
        public Dictionary<Type, List<object>> injectedImplements { get; } = new()
        {
            { typeof(IHitboxProvider), new List<object>() },
            { typeof(IHitboxAttachPoint), new List<object>() },
            { typeof(ICharacterSwapNotifier), new List<object>() }
        };

        public Dictionary<Type, List<ScriptableObject>> RequiredData {get;} = new ()
        {
            {typeof(IPlayerStateEventSubscriber), new List<ScriptableObject>()}
        };


        private IPlayerStateEventSubscriber _eventSubscriber;
        private ICharacterSwapNotifier _characterSwapNotifier;
        private IHitboxProvider _provider;
        private int _targetMask;   // 플레이어 공격이 때릴 대상 레이어. 적 핸들러가 생기면 abstract로 분리한다.
        private readonly Dictionary<HitboxAttachPointType, Transform> _attachPoints = new();
        private readonly List<ActiveHitbox> _actives = new();

        private class ActiveHitbox
        {
            public GameObject instance;
            public Coroutine routine;
            public bool untilFinish;
        }

        void Awake()
        {
            _eventSubscriber = (IPlayerStateEventSubscriber)RequiredData[typeof(IPlayerStateEventSubscriber)][0];
            _eventSubscriber.Subscribe(StateEventCategory.Hitbox, HandleHitbox);
            _eventSubscriber.SubscribeReset(HandleReset);

            _provider = (IHitboxProvider)injectedImplements[typeof(IHitboxProvider)][0];

            _characterSwapNotifier = (ICharacterSwapNotifier)injectedImplements[typeof(ICharacterSwapNotifier)][0];
            _characterSwapNotifier.OnCharacterSwapped += OnCharacterSwapped;

            foreach (var obj in injectedImplements[typeof(IHitboxAttachPoint)])
            {
                var point = (IHitboxAttachPoint)obj;
                _attachPoints[point.Key] = point.Transform;
            }

            _targetMask = LayerMask.GetMask("Enemy", "Gimmick");
        }

        //대원_TODO: 히트박스 켜짐은 애니 진행률이 원하는 진행률을 지났을 때 켜지는 것이라 저프레임(10fps 정도)에선 타이밍이 늦을 수 있음. 
        //체감상 괜찮지만 원하는 타이밍에 진행되는 것이 아니기에 Animation Event로 타이밍을 2중 감시하기
        private void HandleHitbox(IStartData data)
        {
            var hitbox = (IPlayerHitbox)data;

            var instance = _provider.Rent(hitbox.HitboxObject);
            if (instance == null) return;

            if (!_attachPoints.TryGetValue(hitbox.AttachKey, out var parent))
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

            // 전투값과 공격자(충돌 시점의 현재 캐릭터)를 감지 컴포넌트에 주입한다.
            var reporter = _provider.GetReporter(instance);
            if (reporter != null)
            {
                reporter.Setup((IHitboxCombat)data, _characterSwapNotifier.CurrentCharacterObject, _targetMask);
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

        private void HandleReset()
        {
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                var active = _actives[i];
                if (active.untilFinish)
                {
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

        private void OnCharacterSwapped(PlayerCharacterType type)
        {
            foreach (var active in _actives)
            {
                if (active.untilFinish)
                {
                    active.instance.transform.SetParent(null, true);
                }
            }
        }

        private void OnDestroy()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe(StateEventCategory.Hitbox, HandleHitbox);
                _eventSubscriber.UnsubscribeReset(HandleReset);
            }

            if (_characterSwapNotifier != null)
            {
                _characterSwapNotifier.OnCharacterSwapped -= OnCharacterSwapped;
            }

            foreach (var active in _actives)
            {
                if (active.routine != null) StopCoroutine(active.routine);
            }
            _actives.Clear();
        }
    }
}
