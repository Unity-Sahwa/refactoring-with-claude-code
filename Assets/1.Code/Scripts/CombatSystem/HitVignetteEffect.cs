using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Refactoring
{
    // 역할: 외부 호출에 의해 Volume 오브젝트(자신)가 활성화되면 자동으로 Vignette 코루틴 메서드를 호출한다.
    // 흐름: 오브젝트 활성화 → voluem의 weight 0→max(페이드인) → 유지 → max→0(페이드아웃) → 풀이 반납(비활성)
    [RequireComponent(typeof(Volume))]
    public class HitVignetteEffect : MonoBehaviour
    {
        //대원_TODO: HitState(SO 에셋) > Effect > Hit Vignette의 Duration 주의사항. Duration > _fadeInTime + _holdTime + _fadeOutTime
        [SerializeField] private float _maxWeight = 1f;     // 켜졌을 때 최대 weight
        [SerializeField] private float _fadeInTime = 0.05f;  // 0 → max 켜지는 시간
        [SerializeField] private float _holdTime = 0.3f;     // max로 유지하는 시간
        [SerializeField] private float _fadeOutTime = 0.2f;  // max → 0 꺼지는 시간

        private Volume _volume;

        private void Awake()
        {
            _volume = GetComponent<Volume>();
        }

        private void OnEnable()
        {
            _volume.weight = 0f; // 왜: 풀에서 재사용되는 오브젝트라 이전 연출의 weight가 남아있음. 항상 0에서 새로 시작.
            StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            yield return Fade(0f, _maxWeight, _fadeInTime);
            _volume.weight = _maxWeight;
            yield return new WaitForSeconds(_holdTime);
            yield return Fade(_maxWeight, 0f, _fadeOutTime);
            // weight 0으로 쉬고, 실제 비활성/반납은 풀(Duration)이 처리함.
        }

        private IEnumerator Fade(float from, float to, float time)
        {
            //time이 될때까지 계속 weight 값 변경
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                _volume.weight = Mathf.Lerp(from, to, t / time);
                yield return null;
            }
            _volume.weight = to;
        }
    }
}
