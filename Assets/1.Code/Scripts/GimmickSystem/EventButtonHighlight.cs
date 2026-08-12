using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 역할: 화면에 어두운 막을 깔고 지정한 버튼 하나만 그 위로 띄워서 반짝반짝하게 만드는 기믹 이벤트.
    // 안내말 오브젝트를 넣어두면 같이 뜸. 그 버튼을 누르면 알아서 원래대로 돌아감. 시간은 안 멈춤.
    public class EventButtonHighlight : EventData
    {
        [SerializeField] private Button targetButton;    // 강조할 버튼
        [SerializeField] private GameObject dimPanel;    // 화면을 덮는 반투명 검은 이미지. 평소엔 꺼둠. Raycast Target 켜둬야 다른 버튼이 안 눌림
        [SerializeField] private GameObject guideText;   // 안내말 오브젝트. 평소엔 꺼둠. 안 쓸 거면 비워둬도 됨
        [SerializeField] private int sortingOrder = 100; // 어두운 막보다 높아야 버튼이 막 위로 올라옴

        [SerializeField] private float blinkMinAlpha = 0.3f; // 반짝일 때 제일 흐려지는 정도. 0이면 아예 안 보임
        [SerializeField] private float blinkSpeed = 2f;      // 클수록 빨리 깜빡임

        private Canvas _buttonCanvas;
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

            // 버튼만 따로 위로 띄우려면 그 버튼이 자기 Canvas를 갖고 있어야 함. 없으면 여기서 붙여두고 계속 씀.
            // 켤 때 붙이고 끌 때 떼면 Destroy가 프레임 끝에 처리돼서 값이 꼬임. 그래서 한 번만 붙이고 overrideSorting만 켰다 껐다 함.
            _buttonCanvas = SetupCanvas(targetButton.gameObject, sortingOrder);
            _buttonCanvas.overrideSorting = false; // 강조 시작할 때만 켬

            // 반짝임은 이 그룹의 alpha를 오르내리게 해서 만듦. 버튼 안의 글자, 아이콘까지 한꺼번에 흐려짐.
            _buttonGroup = targetButton.GetComponent<CanvasGroup>();
            if (_buttonGroup == null)
            {
                _buttonGroup = targetButton.gameObject.AddComponent<CanvasGroup>();
            }

            if (dimPanel == null)
            {
                Debug.LogError($"{name}: dimPanel이 비어있음. 인스펙터 확인 요망");
            }
            else
            {
                SetupCanvas(dimPanel, sortingOrder - 1); // 강조할 버튼보다 딱 한 칸 아래
            }

            if (guideText != null)
            {
                SetupCanvas(guideText, sortingOrder); // 막보다는 위. 버튼과 같은 층
            }
        }

        // 대상이 자기 Canvas를 갖게 하고 그리는 순서를 직접 매김.
        // 유니티 UI는 하이어라키 순서대로 그려서, 아무리 자리를 옮겨도 부모가 다르면 원하는 위아래가 안 나옴.
        // Canvas를 붙여 번호를 매기면 하이어라키 위치와 상관없이 번호 큰 쪽이 위로 올라옴.
        private Canvas SetupCanvas(GameObject target, int order)
        {
            Canvas canvas = target.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = target.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;

            // Canvas가 따로 생기면 부모 쪽 클릭 감지가 안 닿음. 버튼이 눌리려면, 막이 클릭을 막으려면 여기도 하나 있어야 함.
            if (target.GetComponent<GraphicRaycaster>() == null)
            {
                target.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        public override void Execute()
        {
            if (_showing || targetButton == null)
            {
                return;
            }
            _showing = true;

            if (dimPanel != null)
            {
                dimPanel.SetActive(true);
            }
            if (guideText != null)
            {
                guideText.SetActive(true);
            }

            _buttonCanvas.overrideSorting = true;
            _buttonCanvas.sortingOrder = sortingOrder;

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

            _buttonCanvas.overrideSorting = false;

            if (dimPanel != null)
            {
                dimPanel.SetActive(false);
            }
            if (guideText != null)
            {
                guideText.SetActive(false);
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
