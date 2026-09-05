using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: HitChannel을 구독해, 타격 위치에 이펙트를 켠다.
    // 흐름: 타격 신호 수신 → 풀에서 이펙트 꺼내 위치 지정 → 활성화 → duration 뒤 풀로 반납
    public class HitEffectHandler : MonoBehaviour
    {
        [Preserve, Inject] private HitChannel _hitChannel;

        [Tooltip("타격 지점에 켤 이펙트 프리팹")]
        [SerializeField] private GameObject _hitEffectPrefab;

        [Tooltip("이펙트를 켜 두는 시간 (초)")]
        [SerializeField, Min(0f)] private float _duration = 1f;

        private ObjectPool<GameObject> _pool;
        private WaitForSeconds _wait;
        private IDisposable _hitDisposable;

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(CreateEffect, actionOnRelease: HideEffect, defaultCapacity: 8);
            _wait = new WaitForSeconds(_duration);
        }

        // 구독을 Awake가 아니라 여기서 하는 이유: 꺼진 동안 알림을 받으면 StartCoroutine이 실패해
        // 풀에서 꺼낸 이펙트를 되돌려주지 못하고 그대로 잃어버림.
        private void OnEnable()
        {
            _hitDisposable = _hitChannel.Register(HandleHit);
        }

        private void OnDisable()
        {
            _hitDisposable?.Dispose();
            _hitDisposable = null;
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

        private void HandleHit(HitReport report)
        {
            if (_hitEffectPrefab == null)
            {
                Debug.LogWarning($"{name}: 이펙트 프리팹이 비어 있어 타격 이펙트를 건너뜀.", this);
                return;
            }

            GameObject hitEffectObject = _pool.Get();
            hitEffectObject.transform.position = report.Point;

            // 위치를 잡은 뒤에 켠다. 먼저 켜면 이전 타격 위치에서 한 번 번쩍임.
            hitEffectObject.SetActive(true);
            StartCoroutine(CoReleaseAfter(hitEffectObject));
        }

        private IEnumerator CoReleaseAfter(GameObject hitEffectObject)
        {
            yield return _wait;

            if (hitEffectObject != null)
            {
                _pool.Release(hitEffectObject);
            }
        }
    }
}
