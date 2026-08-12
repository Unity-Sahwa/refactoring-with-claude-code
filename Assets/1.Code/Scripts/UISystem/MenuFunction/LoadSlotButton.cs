using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 고른 슬롯을 불러오는 버튼. 불러오기 안내창의 "예" 버튼에 붙인다.
    // 어느 슬롯인지는 목록 창이 들고 있다.
    [RequireComponent(typeof(Button))]
    public class LoadSlotButton : MonoBehaviour
    {
        // 목록 창과 이 버튼은 서로 다른 프리팹에 있어서 인스펙터로 이어줄 수 없다.
        // AttributeInjector가 씬 오브젝트를 실제 타입 이름으로도 등록하고 꺼져 있는 것까지 모으니 그걸 쓴다.
        [Inject(true)] private SlotListWindow _slotList;

        [Inject(true)] private ISaveSlots _saveSlots;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            if (_slotList == null)
            {
                return;
            }

            _saveSlots?.LoadSlot(_slotList.SelectedIndex);
        }
    }
}
