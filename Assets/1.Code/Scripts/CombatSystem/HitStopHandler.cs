using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: HitChannel을 구독해, 타격 순간 때린 쪽·맞은 쪽의 애니를 잠깐 멈춘다(히트스탑).
    // 흐름: 타격 신호 수신 → 양쪽 Animator 속도 0 → 정해진 시간 뒤 원래 속도 복구
    public class HitStopHandler : MonoBehaviour
    {
        [Preserve, Inject] private HitChannel _hitChannel;

        [Tooltip("때린 쪽 정지 시간 (초)")]
        [SerializeField] private float _attackerFreeze = 0.08f;

        [Tooltip("맞은 쪽 정지 시간 (초)")]
        [SerializeField] private float _targetFreeze = 0.12f;

        // 얼린 애니메이터 → 얼리기 직전 속도
        private readonly Dictionary<Animator, float> _frozen = new();
        private IDisposable _hitDisposable;

        private void Awake()
        {
            if (_hitChannel == null)
            {
                Debug.LogError($"{name}: HitChannel 주입이 안 돼 히트스탑이 동작하지 않음.", this);
                return;
            }

            _hitDisposable = _hitChannel.Register(HandleHit);
        }

        private void OnDisable()
        {
            // 얼린 상태로 꺼지면 애니가 영원히 멈추므로 여기서 원상복구한다.
            foreach (KeyValuePair<Animator, float> pair in _frozen)
            {
                if (pair.Key != null)
                {
                    pair.Key.speed = pair.Value;
                }
            }

            _frozen.Clear();
        }

        private void OnDestroy()
        {
            _hitDisposable?.Dispose();
        }

        private void HandleHit(HitReport report)
        {
            Freeze(report.Attacker, _attackerFreeze);
            Freeze(report.Target, _targetFreeze);
        }

        private void Freeze(GameObject go, float duration)
        {
            if (go == null)
            {
                return;
            }

            Animator anim = go.GetComponentInChildren<Animator>();

            // 이미 얼어있는 상대는 무시
            if (anim == null || _frozen.ContainsKey(anim))
            {
                return;
            }

            _frozen[anim] = anim.speed;
            anim.speed = 0f;
            StartCoroutine(CoUnfreeze(anim, duration));
        }

        private IEnumerator CoUnfreeze(Animator anim, float duration)
        {
            yield return new WaitForSeconds(duration);

            if (!_frozen.Remove(anim, out float prev))
            {
                yield break;
            }

            if (anim != null)
            {
                anim.speed = prev;
            }
        }
    }
}
