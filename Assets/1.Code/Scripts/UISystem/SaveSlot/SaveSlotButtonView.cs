using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 책임: 슬롯 버튼 하나의 겉모습. (무엇을 채울지는 SlotListWindow가 정한다)
    [RequireComponent(typeof(Button))]
    public class SaveSlotButtonView : MonoBehaviour
    {
        // 자리를 이름으로 찾으면 프리팹에서 이름을 바꾸는 순간 에러도 없이 글자만 안 나온다.
        // 인스펙터 연결은 이름을 바꿔도 안 끊기고, 끊기면 인스펙터에서 바로 보인다.
        [SerializeField] private TMP_Text _area;

        [SerializeField] private TMP_Text _date;

        // 빈 칸일 때만 켜는 "기록 없음" 오브젝트.
        [SerializeField] private GameObject _noData;

        public void Fill(string zoneName, string savedTime, bool isEmpty, TMP_FontAsset font)
        {
            SetLabel(_area, isEmpty ? string.Empty : zoneName, font);
            SetLabel(_date, isEmpty ? string.Empty : savedTime, font);

            if (_noData != null)
            {
                _noData.SetActive(isEmpty);
            }

            GetComponent<Button>().interactable = !isEmpty;
        }

        // 언어마다 쓰는 글꼴이 달라서 글자와 글꼴을 같이 갈아 끼운다.
        private void SetLabel(TMP_Text label, string text, TMP_FontAsset font)
        {
            if (label == null)
            {
                return;
            }

            label.text = text;

            if (font != null)
            {
                label.font = font;
            }
        }
    }
}
