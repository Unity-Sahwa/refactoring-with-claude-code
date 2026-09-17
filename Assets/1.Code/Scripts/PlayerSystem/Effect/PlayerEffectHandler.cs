using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 상태 이벤트로 받은 데이터(위치·시간)대로 IEffectProvider가 준 이펙트를 켠다.
    public class PlayerEffectHandler : MonoBehaviour
    {
        [Preserve, Inject] private IPlayerStateEventSubscriber _eventSubscriber;
        [Preserve, Inject] private IEffectProvider _provider;
        [Preserve, Inject] private List<IEffectAttachPoint> _effectAttachPoints;
        private readonly Dictionary<EffectAttachPointType, Transform> _attachPoints = new();
        private readonly List<ActiveEffect> _actives = new();
        private IDisposable _effectEventDisposable;

        private class ActiveEffect
        {
            public GameObject Instance;
            public Coroutine Routine;
            public bool UntilFinish;
        }

        private void Awake()
        {
            if (_eventSubscriber == null || _provider == null || _effectAttachPoints == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerEffectHandler)}: 필수 의존 주입 실패");
            }

            _effectEventDisposable = _eventSubscriber.Register(StateEventCategory.Effect, HandleEffect, HandleReset);

            foreach (var obj in _effectAttachPoints)
            {
                _attachPoints[obj.Key] = obj.Transform;
            }
        }

        private void HandleEffect(IStartData data)
        {
            if (data is not IPlayerEffect effect)
            {
                Debug.LogError($"[PlayerEffectHandler] IPlayerEffect가 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            GameObject instance = RentAttachedEffect(effect);
            if (instance == null)
            {
                return;
            }

            StartActiveEffect(instance, effect);
        }

        private GameObject RentAttachedEffect(IPlayerEffect effect)
        {
            GameObject instance = _provider.Rent(effect.EffectId);
            if (instance == null)
            {
                return null;
            }

            if (!_attachPoints.TryGetValue(effect.AttachKey, out Transform parent))
            {
                _provider.Return(instance);
                return null;
            }

            instance.transform.SetParent(parent, false);
            return instance;
        }

        private void StartActiveEffect(GameObject instance, IPlayerEffect effect)
        {
            ApplyTransform(instance.transform, effect);
            instance.SetActive(true);

            var active = new ActiveEffect { Instance = instance, UntilFinish = effect.UntilFinish };
            active.Routine = StartCoroutine(CoRunEffect(active, effect));
            _actives.Add(active);
        }

        private void ApplyTransform(Transform effectTransform, IPlayerEffect effect)
        {
            effectTransform.localPosition = effect.Position;
            effectTransform.localRotation = Quaternion.Euler(effect.Rotation);
            effectTransform.localScale = effect.Scale == Vector3.zero ? Vector3.one : effect.Scale;
        }

        private IEnumerator CoRunEffect(ActiveEffect active, IPlayerEffect effect)
        {
            if (effect.StopInPlace)
            {
                float stopTime = Mathf.Clamp(effect.StopTime, 0f, effect.Duration);
                yield return new WaitForSeconds(stopTime);
                active.Instance.transform.SetParent(null, true);
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
        private void HandleReset(CloseEventType reason)
        {
            for (int i = _actives.Count - 1; i >= 0; i--)
            {
                ResolveActiveOnReset(_actives[i]);
            }
        }

        private void ResolveActiveOnReset(ActiveEffect active)
        {
            if (active.UntilFinish)
            {
                active.Instance.transform.SetParent(null, true);
                return;
            }

            if (active.Routine != null)
            {
                StopCoroutine(active.Routine);
            }
            active.Instance.SetActive(false);
            FinishEffect(active);
        }

        private void FinishEffect(ActiveEffect active)
        {
            _actives.Remove(active);
            _provider.Return(active.Instance);
        }

        private void OnDestroy()
        {
            _effectEventDisposable?.Dispose();

            foreach (var active in _actives)
            {
                if (active.Routine != null)
                {
                    StopCoroutine(active.Routine);
                }
            }
            _actives.Clear();
        }
    }
}