using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 역할: 지정한 오브젝트들 켜고, 지정한 오브젝트들 끄고, 타겟 버튼을 반짝이게 만드는 기믹 이벤트.
    // 그 버튼을 누르면 알아서 원래대로 돌아감.
    public class EventButtonGuide : EventData
    {
        [SerializeField] private Button targetButton;           // 강조할 버튼
        [SerializeField] private GameObject[] objectsToEnable;  // 강조 시작할 때 켤 오브젝트들 (예: dimPanel, guideText)
        [SerializeField] private GameObject[] objectsToDisable; // 강조 시작할 때 끌 오브젝트들 (예: 이동/카메라 조이스틱, 다른 버튼)

        [SerializeField] private float blinkMinAlpha = 0.3f; // 반짝일 때 제일 흐려지는 정도. 0이면 아예 안 보임
        [SerializeField] private float blinkSpeed = 2f;      // 클수록 빨리 깜빡임

        private CanvasGroup _buttonGroup;
        private Coroutine _blinking;
        private bool _showing;

        private void Awake()
        {
            if (targetButton == null)
            {
                Debug.LogError($"{name}: targetButton이 비어있음. 인스펙터 확인 요망");
                return;
            }

            // 반짝임은 이 그룹의 alpha를 오르내리게 해서 만듦. 버튼 안의 글자, 아이콘까지 한꺼번에 흐려짐.
            _buttonGroup = targetButton.GetComponent<CanvasGroup>();
            if (_buttonGroup == null)
            {
                _buttonGroup = targetButton.gameObject.AddComponent<CanvasGroup>();
            }
        }

        public override void Execute()
        {
            if (_showing || targetButton == null)
            {
                return;
            }
            _showing = true;

            SetActiveAll(objectsToEnable, true);
            SetActiveAll(objectsToDisable, false);

            _blinking = StartCoroutine(Blink());
            targetButton.onClick.AddListener(Hide);
        }

        // 강조한 버튼을 누르면 자동으로 불림.
        public void Hide()
        {
            if (!_showing)
            {
                return;
            }
            _showing = false;

            targetButton.onClick.RemoveListener(Hide);

            if (_blinking != null)
            {
                StopCoroutine(_blinking);
                _blinking = null;
            }
            _buttonGroup.alpha = 1f; // 반짝이던 도중에 멈추면 흐린 채로 굳음. 원래대로 되돌림

            SetActiveAll(objectsToEnable, false);
            SetActiveAll(objectsToDisable, true);
        }

        private static void SetActiveAll(GameObject[] objects, bool active)
        {
            if (objects == null)
            {
                return;
            }
            foreach (var obj in objects)
            {
                if (obj != null)
                {
                    obj.SetActive(active);
                }
            }
        }

        private IEnumerator Blink()
        {
            while (true)
            {
                // PingPong은 0 -> 1 -> 0 을 계속 왕복함. 그 값으로 흐림<->또렷함을 오감.
                float t = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
                _buttonGroup.alpha = Mathf.Lerp(blinkMinAlpha, 1f, t);
                yield return null;
            }
        }
    }
}
