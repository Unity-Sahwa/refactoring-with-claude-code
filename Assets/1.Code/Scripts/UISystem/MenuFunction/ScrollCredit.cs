using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    public class ScrollCredit : MonoBehaviour
    {
        [SerializeField] float scrollSpeed;
        ScrollRect scrollRect;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }


        private void OnEnable()
        {
            StopAllCoroutines();
            scrollRect.verticalNormalizedPosition = 1f;
            StartCoroutine(CoScroll());
        }

        private IEnumerator CoScroll()
        {
            while (scrollRect.verticalNormalizedPosition > 0f)
            {
                scrollRect.verticalNormalizedPosition =
                    Mathf.Clamp01(scrollRect.verticalNormalizedPosition - scrollSpeed * Time.unscaledDeltaTime);
                
                yield return null;
            }
        }
    }
}