using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 설정창. 안쪽 탭(게임플레이·소리·언어·플랫폼)은 창 스택에 안 넣고 여기서 하나만 켠다.
    public class SettingsWindow : UIWindow
    {
        // 설정값 주인들. 여기선 감도인지 소리인지 모르고 "저장해"만 시킨다.
        [Preserve, Inject(true)] private List<ISettingsHolder> _holders;

        private UIWindow[] _tabs;

        private void Awake()
        {
            _tabs = GetComponentsInChildren<UIWindow>(true);
        }

        // 창을 닫을 때 한 번만 파일에 쓴다. 슬라이더를 끄는 동안 매번 쓰면 파일 쓰기가 너무 잦다.
        public override void Close()
        {
            base.Close();

            if (_holders == null)
            {
                return;
            }

            foreach (ISettingsHolder holder in _holders)
            {
                holder.Save();
            }
        }

        // 탭을 창 스택에 넣으면 바꿀 때마다 뒤로가기가 한 겹씩 쌓여서, 여기서 직접 하나만 켠다.
        public void ShowTab(WindowType id)
        {
            foreach (UIWindow tab in _tabs)
            {
                // 자기 자신(설정창)은 탭이 아니라서 건너뛴다.
                if (tab == this)
                {
                    continue;
                }

                if (tab.Id == id)
                {
                    tab.Open();
                }
                else
                {
                    tab.Close();
                }
            }
        }
    }
}
