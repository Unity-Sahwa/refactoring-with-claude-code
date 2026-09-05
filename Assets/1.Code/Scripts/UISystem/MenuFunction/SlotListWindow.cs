using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Refactoring
{
    // 책임: 저장된 슬롯 목록을 보여주는 창. 슬롯 개수가 정해져 있지 않아서 열 때마다 버튼을 찍어낸다.
    // 흐름: 창 열림 → 기존 버튼 지우기 → 슬롯 수만큼 버튼 생성 → 누르면 확인창 열기
    public class SlotListWindow : UIWindow
    {
        [SerializeField] private GameObject _slotButtonPrefab;

        // 찍어낸 버튼이 들어갈 부모(세로 목록 오브젝트).
        [SerializeField] private Transform _slotParent;

        [Preserve, Inject(true)] private ISaveSlots _saveSlots;
        [Preserve, Inject(true)] private ILanguageSettings _language;

        // 슬롯 버튼은 창을 열 때 찍어내서 ButtonClickSound의 목록에 안 들어간다. 만들 때 직접 달아준다.
        [Preserve, Inject(true)] private ButtonClickSound _clickSound;

        private UIRoot _root;

        // 방금 고른 슬롯 번호. 불러오기 확인창이 이 값을 보고 불러온다.
        public int SelectedIndex { get; private set; }

        private void Awake()
        {
            _root = GetComponentInParent<UIRoot>(true);
        }

        // 자동저장으로 목록이 바뀌어도 다시 열면 최신이라 변경 이벤트를 안 듣는다.
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

            // 글자를 어느 자리에 넣을지는 버튼 프리팹이 스스로 안다. 여기선 내용만 넘긴다.
            SaveSlotButtonView view = spawnedObject.GetComponent<SaveSlotButtonView>();

            if (view != null)
            {
                // 저장할 때 적어둔 값이 곧 번역 표의 키다. 그대로 넣으면 지금 언어의 지역 이름이 나온다.
                string zoneName = _language != null ? _language.GetText(slot.SavePointKey) : slot.SavePointKey;

                view.Fill(zoneName, slot.SavedTime, slot.IsEmpty, _language?.GetFont());
            }

            // 버튼마다 자기 슬롯 번호를 기억해뒀다가, 눌리면 그 번호를 선택해두고 확인창을 띄운다.
            int index = slot.Index;
            Button button = spawnedObject.GetComponent<Button>();

            button.onClick.AddListener(() => HandleSlotClicked(index));
            button.onClick.AddListener(() => _clickSound?.PlayClick());
        }

        private void HandleSlotClicked(int index)
        {
            SelectedIndex = index;
            _root.OpenWindow(WindowType.LoadConfirm);
        }
    }
}
