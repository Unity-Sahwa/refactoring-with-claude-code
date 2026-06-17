using System;
using System.Collections;
using System.Collections.Generic;
using Refactoring;
using UnityEngine;

namespace Refactoring
{
    public class PlayerEffectHandler : MonoBehaviour
    {
        [Inject] private IPlayerStateEventSubscriber _eventSubscriber;
        [Inject]private ICharacterSwapNotifier _characterSwapNotifier;
        [Inject] private IEffectProvider _provider;
        [Inject] private List<IEffectAttachPoint> _effectAttachPoints;
        private readonly Dictionary<EffectAttachPointType, Transform> _attachPoints = new();
        private readonly List<ActiveEffect> _actives = new();

        private class ActiveEffect
        {
            public GameObject instance;
            public Coroutine routine;
            public bool untilFinish;
        }

        void Awake()
        {
            _eventSubscriber.Subscribe(StateEventCategory.Effect, HandleEffect);
            _eventSubscriber.SubscribeReset(HandleReset);

            _characterSwapNotifier.OnCharacterSwapped += OnCharacterSwapped;

            foreach (var obj in _effectAttachPoints)
            {
                _attachPoints[obj.Key] = obj.Transform;
            }
        }

        private void HandleEffect(IStartData data)
        {
            var effect = (IPlayerEffect)data;

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
            //대원_TODO: UntilFinish가 적용된 상태에서 캐릭터 스왑하면 이펙트가 캐릭터 자식으로 존재해 함께 비활성화되는 문제
            //UtilFinish가 적용되면 reset되는 순간 밖으로 꺼낼까?
            
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
                FinishEffect(active);
            }
        }

        private void FinishEffect(ActiveEffect active)
        {
            _actives.Remove(active);
            _provider.Return(active.instance);
        }

        private void OnCharacterSwapped(PlayerCharacterType type)
        {
            foreach (var active in _actives)
            {
                if(active.untilFinish)
                {
                    active.instance.transform.SetParent(null,true);
                }
            }
        }

        private void OnDestroy()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe(StateEventCategory.Effect, HandleEffect);
                _eventSubscriber.UnsubscribeReset(HandleReset);
            }

            if(_characterSwapNotifier != null)
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