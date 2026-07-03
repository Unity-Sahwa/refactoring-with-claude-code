using UnityEngine;

namespace Refactoring
{
    // 모든 창에 붙는 범용 부품. 자기 이름표를 들고, 열기/닫기 = 켜기/끄기만 한다.
    public class UIWindow : MonoBehaviour, IWindow
    {
        [SerializeField]
        private WindowId _id;

        public WindowId Id => _id;

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
