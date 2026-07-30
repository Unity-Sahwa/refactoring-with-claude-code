using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 버튼마다 붙는 부품. 인스펙터에서 동작을 고르고, 눌리면 UIRoot에 요청만 넘긴다.
    // 확인이 필요한 버튼은 그냥 해당 확인창을 여는 것으로 끝난다(확인창이 자기 일을 안다).
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour
    {
        public enum ButtonAction
        {
            OpenWindow,
            Close,
        }

        [SerializeField]
        private ButtonAction _action;

        // _action이 OpenWindow일 때만 사용하는 열 대상
        [SerializeField]
        private WindowId _targetWindow;

        private UIRoot _root;

        private void Awake()
        {
            // UI는 캔버스 트리라, 위로 거슬러 올라가 총괄(UIRoot)을 찾는다. 슬롯 연결 없음.
            _root = GetComponentInParent<UIRoot>(true);
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            if (_action == ButtonAction.OpenWindow)
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
