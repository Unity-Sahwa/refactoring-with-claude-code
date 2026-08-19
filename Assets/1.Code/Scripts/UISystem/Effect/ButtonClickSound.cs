using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 씬에 있는 버튼 전부에 같은 클릭 소리를 달아준다.
    // 버튼마다 부품을 붙이면 버튼 수만큼 손이 가고, 하나 빠뜨려도 티가 안 나서 한 곳에서 단다.
    //
    // 씬에 하나만 놓는다. 둘 이상 놓으면 버튼 하나에 소리가 두 번 붙는다.
    public class ButtonClickSound : MonoBehaviour
    {
        [Inject] private AudioChannel _audioChannel;

        // 주입기가 씬의 MonoBehaviour를 전부 등록해서, 버튼도 이렇게 한꺼번에 받을 수 있다(꺼져 있는 것 포함).
        [Inject] private List<Button> _buttons;

        // Awake가 아니라 Start인 이유: 주입이 Awake에 일어나서, Awake에 읽으면 아직 비어 있다.
        private void Start()
        {
            foreach (Button button in _buttons)
            {
                // NoClickSound가 붙은 버튼(예: 모바일 공격 버튼)은 제외한다.
                if (button.GetComponent<NoClickSound>() != null) continue;

                button.onClick.AddListener(PlayClick);
            }
        }

        // 창을 열 때 찍어내는 버튼(불러오기 슬롯)은 위 목록에 없다. 그쪽에서 이걸 직접 부른다.
        public void PlayClick()
        {
            _audioChannel.RaisePlay(AudioPlayRequest.Of(AudioId.UIClick));
        }
    }
}
