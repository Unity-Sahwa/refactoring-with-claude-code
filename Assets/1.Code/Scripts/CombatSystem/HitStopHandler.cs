using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: HitChannel을 구독해, 타격 성공 순간 때린 놈/맞은 놈의 애니를 잠깐 멈춘다(히트스탑).
    public class HitStopHandler : MonoBehaviour
    {
        [Inject] private HitChannel _hitChannel;
        [SerializeField] private float _attackerFreeze = 0.08f; // 때린 놈(플레이어) 정지 시간
        [SerializeField] private float _targetFreeze = 0.12f;   // 맞은 놈 정지 시간
        private readonly Dictionary<Animator, float> _frozen = new(); // 얼린 애니메이터 → 얼리기 직전 속도
        private IDisposable _hitDisposable;
        private WaitForSecondsRealtime _wait;

        private void Awake()
        {
            if (_hitChannel == null) 
            {
                return;
            }
            
            _hitDisposable = _hitChannel.Register(HandleHit);
        }

        private void OnDestroy()
        {
           _hitDisposable?.Dispose(); 
        }

        private void OnDisable()
        {
            //오브젝트 비활성화되는 경우, 원상복구
            foreach (var pair in _frozen)
            {
                if (pair.Key != null) 
                {
                    pair.Key.speed = pair.Value;
                }
            }
            _frozen.Clear();
        }

        //타격 성공시 타격자/피격자 모두 얼림
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

            var anim = go.GetComponentInChildren<Animator>();
            
            // 이미 얼어있는 상대는 무시
            if (anim == null || _frozen.ContainsKey(anim)) 
            {
                return;
            }

            _frozen[anim] = anim.speed;
            anim.speed = 0f;
            StartCoroutine(Unfreeze(anim, duration));
        }

        
        private IEnumerator Unfreeze(Animator anim, float duration)
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
