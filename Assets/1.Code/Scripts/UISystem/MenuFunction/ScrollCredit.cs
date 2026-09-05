using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Refactoring
{
    // 책임: 크레딧 창이 열리면 맨 위부터 아래로 자동으로 굴려 내린다.
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollCredit : MonoBehaviour
    {
        [Tooltip("1초에 굴러가는 양(0~1 비율 기준)")]
        [SerializeField] private float _scrollSpeed;

        private ScrollRect _scrollRect;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        // 창을 다시 열면 처음부터 굴러야 해서, 이전 코루틴을 끊고 맨 위로 되돌린다.
        private void OnEnable()
        {
            StopAllCoroutines();
            _scrollRect.verticalNormalizedPosition = 1f;
            StartCoroutine(CoScroll());
        }

        private IEnumerator CoScroll()
        {
            while (_scrollRect.verticalNormalizedPosition > 0f)
            {
                // 메뉴에서는 시간이 멈춰 있을 수 있어서 unscaledDeltaTime을 쓴다.
                _scrollRect.verticalNormalizedPosition =
                    Mathf.Clamp01(_scrollRect.verticalNormalizedPosition - _scrollSpeed * Time.unscaledDeltaTime);

                yield return null;
            }
        }
    }
}
