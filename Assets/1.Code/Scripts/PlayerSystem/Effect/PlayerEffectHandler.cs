using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 상태 이벤트의 알림과 함께 전달받은 데이터(위치, 시간범위 등)로 IEffectProvider으로부터 제공된 이펙트를 활성화시킨다.

    public class PlayerEffectHandler : MonoBehaviour
    {
        [Inject] private IPlayerStateEventSubscriber _eventSubscriber;
        [Inject] private IEffectProvider _provider;
        [Inject] private List<IEffectAttachPoint> _effectAttachPoints;
        private readonly Dictionary<EffectAttachPointType, Transform> _attachPoints = new();
        private readonly List<ActiveEffect> _actives = new();
        private IDisposable _effectEventDisposable;

        private class ActiveEffect
        {
            public GameObject instance;
            public Coroutine routine;
            public bool untilFinish;
        }

        void Awake()
        {
            _effectEventDisposable = _eventSubscriber.RegisterEventShot(StateEventCategory.Effect, HandleEffect, HandleReset);

            foreach (var obj in _effectAttachPoints)
            {
                _attachPoints[obj.Key] = obj.Transform;
            }
        }

        private void HandleEffect(PlayerCharacter source, IStartData data)
        {
            if (data is not IPlayerEffect effect)
            {
                Debug.LogError($"[PlayerEffectHandler] IPlayerEffect가 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            var instance = _provider.Rent(effect.EffectObject);
            if (instance == null) return;
        
            if (!_attachPoints.TryGetValue(effect.AttachKey, out var parent))
            {
                _provider.Return(instance);
                return;
            }

            var t = instance.transform;
            t.SetParent(parent, false);
            t.localPosition = effect.Position;
            t.localRotation = Quaternion.Euler(effect.Rotation);
            if (effect.Scale == Vector3.zero) t.localScale = Vector3.one;
            else t.localScale = effect.Scale;
            
            instance.SetActive(true);

            var active = new ActiveEffect { instance = instance, untilFinish = effect.UntilFinish };
            active.routine = StartCoroutine(CoRunEffect(active, effect));
            _actives.Add(active);
        }

        private IEnumerator CoRunEffect(ActiveEffect active, IPlayerEffect effect)
        {
            if (effect.StopInPlace)
            {
                float stopTime = Mathf.Clamp(effect.StopTime, 0f, effect.Duration);
                yield return new WaitForSeconds(stopTime);
                active.instance.transform.SetParent(null, true);
                yield return new WaitForSeconds(effect.Duration - stopTime);
            }
            else
            {
                yield return new WaitForSeconds(effect.Duration);
            }

            FinishEffect(active);
        }

        // 왜: untilFinish 이펙트는 상태가 끝나도 살아남아야 한다. 다만 캐릭터 자식으로 붙어 있으면
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
                FinishEffect(active);
            }
        }

        private void FinishEffect(ActiveEffect active)
        {
            _actives.Remove(active);
            _provider.Return(active.instance);
        }

        private void OnDestroy()
        {
            _effectEventDisposable?.Dispose();

            foreach (var active in _actives)
            {
                if (active.routine != null) StopCoroutine(active.routine);
            }
            _actives.Clear();
        }
    }
}