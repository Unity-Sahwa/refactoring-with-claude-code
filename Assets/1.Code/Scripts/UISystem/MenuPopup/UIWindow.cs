using UnityEngine;

namespace Refactoring
{
    // 책임: 모든 창에 붙는 범용 부품. 자기 이름표를 들고, 열기/닫기 = 켜기/끄기만 한다.
    public class UIWindow : MonoBehaviour, IWindow
    {
        [SerializeField] private WindowType _id;

        public WindowType Id => _id;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        // 닫힐 때 더 할 일이 있는 창(설정창의 저장 등)이 있어서 물려받아 고칠 수 있게 둔다.
        public virtual void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
