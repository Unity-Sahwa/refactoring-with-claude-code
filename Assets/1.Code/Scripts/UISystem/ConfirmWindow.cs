using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 예/아니오 확인창의 공통 껍데기. 예 버튼을 잡아주고, 누르면 창을 닫은 뒤 자기 일을 시킨다.
    // 무슨 일을 하는지는 이걸 물려받은 창들이 각자 채운다.
    // 자식 창에서 Awake를 다시 만들면 여기 Awake가 안 불리니 주의.
    public abstract class ConfirmWindow : UIWindow
    {
        [SerializeField]
        private Button _yesButton;

        protected UIRoot Root { get; private set; }

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
