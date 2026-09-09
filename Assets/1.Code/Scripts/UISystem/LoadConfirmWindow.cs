using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: "이 슬롯을 불러올까요?" 창. 어느 슬롯인지는 목록 창이 들고 있다.
    public class LoadConfirmWindow : ConfirmWindow
    {
        [SerializeField] private SlotListWindow _slotList;

        [Preserve, Inject(true)] private ISaveSlots _saveSlots;

        protected override void RunYes()
        {
            _saveSlots?.LoadSlot(_slotList.SelectedIndex);
        }
    }
}
