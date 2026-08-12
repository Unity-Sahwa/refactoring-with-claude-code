using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // 저장된 슬롯 목록을 보여주는 창. 슬롯 개수가 정해져 있지 않아서 열 때마다 버튼을 찍어낸다.
    // 게임 중 자동저장으로 목록이 바뀌어도 다시 열면 최신이라 이벤트를 안 듣는다.
    public class SlotListWindow : UIWindow
    {
        [SerializeField]
        private GameObject _slotButtonPrefab;

        // 찍어낸 버튼이 들어갈 부모(세로 목록 오브젝트).
        [SerializeField]
        private Transform _slotParent;

        [Inject(true)] private ISaveSlots _saveSlots;

        private UIRoot _root;

        // 방금 고른 슬롯 번호. 불러오기 확인창이 이 값을 보고 불러온다.
        public int SelectedIndex { get; private set; }

        private void Awake()
        {
            _root = GetComponentInParent<UIRoot>(true);
        }

        private void OnEnable()
        {
            BuildSlots();
        }

        private void BuildSlots()
        {
            for (int i = _slotParent.childCount - 1; i >= 0; i--)
            {
                Destroy(_slotParent.GetChild(i).gameObject);
            }

            if (_saveSlots == null)
            {
                return;
            }

            foreach (SaveSlotInfo slot in _saveSlots.GetSlots())
            {
                CreateSlotButton(slot);
            }
        }

        private void CreateSlotButton(SaveSlotInfo slot)
        {
            GameObject spawnedObject = Instantiate(_slotButtonPrefab, _slotParent);

            TMP_Text label = spawnedObject.GetComponentInChildren<TMP_Text>();

            if (label != null)
            {
                label.text = slot.IsEmpty ? "-" : $"{slot.ZoneName}  {slot.SavedTime}";
            }

            Button button = spawnedObject.GetComponent<Button>();
            button.interactable = !slot.IsEmpty;

            // 버튼마다 자기 슬롯 번호를 기억해뒀다가, 눌리면 그 번호를 선택해두고 확인창을 띄운다.
            int index = slot.Index;
            button.onClick.AddListener(() => HandleSlotClicked(index));
        }

        private void HandleSlotClicked(int index)
        {
            SelectedIndex = index;
            _root.OpenWindow(WindowId.LoadConfirm);
        }
    }
}
