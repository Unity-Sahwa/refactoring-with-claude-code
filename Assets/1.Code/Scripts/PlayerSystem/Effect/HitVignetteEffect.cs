using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Refactoring
{
    // 책임: 자신이 활성화되면 Volume weight를 넣었다 빼는 비네트 연출을 재생한다.
    // 흐름: 활성화 → weight 0→max(페이드인) → 유지 → max→0(페이드아웃) → 풀이 반납(비활성)
    [RequireComponent(typeof(Volume))]
    public class HitVignetteEffect : MonoBehaviour
    {
        [Tooltip("켜졌을 때 최대 weight")]
        [SerializeField] private float _maxWeight = 1f;

        [Tooltip("0 → max 로 켜지는 시간 (초)")]
        [SerializeField] private float _fadeInTime = 0.05f;

        [Tooltip("max 로 유지하는 시간 (초)")]
        [SerializeField] private float _holdTime = 0.3f;

        [Tooltip("max → 0 으로 꺼지는 시간 (초)")]
        [SerializeField] private float _fadeOutTime = 0.2f;

        private Volume _volume;

        private void Awake()
        {
            _volume = GetComponent<Volume>();
        }

        private void OnEnable()
        {
            // 풀에서 재사용되는 오브젝트라 이전 연출의 weight가 남아있음. 항상 0에서 새로 시작.
            _volume.weight = 0f;
            StartCoroutine(CoShow());
        }

        private IEnumerator CoShow()
        {
            yield return CoFade(0f, _maxWeight, _fadeInTime);

            _volume.weight = _maxWeight;
            yield return new WaitForSeconds(_holdTime);

            // weight 0으로 쉬고, 실제 비활성/반납은 풀(Duration)이 처리함.
            yield return CoFade(_maxWeight, 0f, _fadeOutTime);
        }

        private IEnumerator CoFade(float from, float to, float time)
        {
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                _volume.weight = Mathf.Lerp(from, to, t / time);
                yield return null;
            }

            _volume.weight = to;
        }
    }
}
