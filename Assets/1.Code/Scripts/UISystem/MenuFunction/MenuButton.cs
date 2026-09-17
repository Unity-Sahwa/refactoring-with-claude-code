using System;
using UnityEngine;
using UnityEngine.Scripting;
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

        [Preserve, Inject] private IUIRoot _root;

        private void Awake()
        {
            if (_root == null)
            {
                throw new InvalidOperationException($"{nameof(MenuButton)}: 필수 의존 주입 실패");
            }

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
