using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 책임: 인스펙터에서 고른 동작을 UIRoot에 요청만 넘긴다. (확인이 필요한 버튼은 확인창을 여는 것으로 끝난다)
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour
    {
        public enum ButtonActionType
        {
            OpenWindow,
            Close,
        }

        [SerializeField] private ButtonActionType _action;

        // _action이 OpenWindow일 때만 사용하는 열 대상
        [SerializeField] private WindowType _targetWindow;

        private UIRoot _root;

        private void Awake()
        {
            // UI는 캔버스 트리라, 위로 거슬러 올라가 총괄(UIRoot)을 찾는다. 슬롯 연결 없음.
            _root = GetComponentInParent<UIRoot>(true);
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            if (_action == ButtonActionType.OpenWindow)
            {
                _root.OpenWindow(_targetWindow);
            }
            else
            {
                _root.CloseTop();
            }
        }
    }
}
