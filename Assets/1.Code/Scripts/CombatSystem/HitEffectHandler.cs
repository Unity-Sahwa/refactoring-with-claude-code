using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Refactoring
{
    // 역할: HitChannel을 구독해, 타격 신호를 받아 타격 위치에 이펙트를 활성화한다.
    public class HitEffectHandler : MonoBehaviour
    {
        [Inject] private HitChannel _hitChannel;
        [SerializeField] private GameObject _hitEffectPrefab;
        [SerializeField, Min(0f)] private float _duration = 1f;

        private ObjectPool<GameObject> _pool;
        private WaitForSeconds _wait;
        private IDisposable _hitDisposable;

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(CreateEffect, actionOnRelease: HideEffect, defaultCapacity: 8);
            _wait = new WaitForSeconds(_duration);
        }

        private GameObject CreateEffect()
        {
            GameObject go = Instantiate(_hitEffectPrefab, transform);
            go.SetActive(false);
            return go;
        }

        // 풀이 이펙트를 돌려받을 때 부른다.
        private void HideEffect(GameObject hitEffectPrefab)
        {
            hitEffectPrefab.SetActive(false);
        }

        // 구독을 Awake가 아니라 여기서 하는 이유: 꺼진 동안 알림을 받으면 StartCoroutine이 실패해
        // 풀에서 꺼낸 이펙트를 되돌려주지 못하고 그대로 잃어버림.
        private void OnEnable()
        {
            _hitDisposable = _hitChannel.Register(HandleHit);
        }

        private void OnDisable()
        {
            _hitDisposable?.Dispose(); _hitDisposable = null; 
        }

        // 타격 성공 시 그 지점에 이펙트 하나 켜고, duration 뒤 풀로 반납한다.
        private void HandleHit(HitReport report)
        {
            if (_hitEffectPrefab == null) 
            {
                return;
            }

            GameObject hitEffectObject = _pool.Get();
            hitEffectObject.transform.position = report.Point;
            hitEffectObject.SetActive(true); // 위치를 잡은 뒤에 켠다. 먼저 켜면 이전 타격 위치에서 한 번 번쩍임.
            StartCoroutine(ReleaseAfter(hitEffectObject));
        }

        private IEnumerator ReleaseAfter(GameObject hitEffectObject)
        {
            yield return _wait;
            
            if (hitEffectObject != null)
            {
                _pool.Release(hitEffectObject);
            } 
        }
    }
}