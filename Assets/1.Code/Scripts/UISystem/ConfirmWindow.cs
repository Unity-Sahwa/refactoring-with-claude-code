using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 책임: 예/아니오 확인창의 공통 껍데기. (예를 눌렀을 때 할 일은 물려받은 창이 채운다)
    // 흐름: 예 버튼 누름 → 창 닫기 → RunYes 실행
    public abstract class ConfirmWindow : UIWindow
    {
        [SerializeField] private Button _yesButton;

        protected UIRoot Root { get; private set; }

        // 자식 창에서 Awake를 다시 만들면 여기 Awake가 안 불려 예 버튼이 죽는다.
        private void Awake()
        {
            Root = GetComponentInParent<UIRoot>(true);
            _yesButton.onClick.AddListener(HandleYesClicked);
        }

        private void HandleYesClicked()
        {
            // 창을 먼저 닫는다. 씬 전환처럼 돌아오지 않는 일이 섞여 있어서 순서가 중요하다.
            Root.CloseTop();
            RunYes();
        }

        // 예를 눌렀을 때 실제로 할 일.
        protected abstract void RunYes();
    }
}
