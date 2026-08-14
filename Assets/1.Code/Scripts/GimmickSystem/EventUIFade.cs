using System.Collections;
using UnityEngine;

namespace Refactoring
{
    // 역할: 이 스크립트가 붙은 UI의 CanvasGroup alpha를 서서히 바꾸는 기믹 이벤트.
    // EnterTrigger나 StartTrigger 같은 데서 Execute()로 불러줘야 돈다.
    [RequireComponent(typeof(CanvasGroup))]
    public class EventUIFade : EventData
    {
        [SerializeField] private bool fadeIn = true;       // true: alpha 0->1 (검은 패널이면 점점 어두워짐) / false: alpha 1->0 (점점 밝아짐)
        [SerializeField] private float fadeInDuration = 1f;       // 들어오는 데 걸리는 시간
        [SerializeField] private float holdDuration = 0f;   // 0 이하면 들어온 상태로 그냥 둠. 0보다 크면 이만큼 버틴 뒤 되돌아가고 오브젝트를 끔
        [SerializeField] private float fadeOutDuration = 0f; // 되돌아가는 데 걸리는 시간

        private CanvasGroup _canvasGroup;
        private Coroutine _running;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void Execute()
        {
            // 이미 돌고 있으면 끊고 처음부터 다시. 두 개가 같이 alpha를 만지면 값이 튐.
            if (_running != null)
            {
                StopCoroutine(_running);
            }

            // 꺼진 채로 씬에 놔둔 오브젝트라 여기서 켜야 코루틴이 돌아감. SetActive(true)는 Awake를 그 자리에서 부르니 _canvasGroup도 채워짐.
            gameObject.SetActive(true);
            _running = StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            float from = fadeIn ? 0f : 1f;
            float to = fadeIn ? 1f : 0f;

            yield return Fade(from, to, fadeInDuration);

            if (holdDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(holdDuration);
                yield return Fade(to, from, fadeOutDuration);

                // SetActive(false)가 코루틴을 끊어버리니 뒷정리를 먼저 해둠.
                _running = null;
                gameObject.SetActive(false);
                yield break;
            }

            _running = null;
        }

        private IEnumerator Fade(float from, float to, float time)
        {
            _canvasGroup.alpha = from;

            // time이 0 이하면 나누기가 깨지니 바로 끝값으로 놓고 끝냄.
            if (time > 0f)
            {
                float elapsed = 0f;
                while (elapsed < time)
                {
                    elapsed += Time.unscaledDeltaTime; // 연출이라 게임이 멈춰도 돌아가야 함
                    _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / time);
                    yield return null;
                }
            }

            _canvasGroup.alpha = to;
        }
    }
}
