using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Refactoring
{
    // 조작키 버튼 하나. 어떤 액션인지만 들고 있고, 누르면 다음에 누른 키로 바꾼다.
    // 이동처럼 키 네 개가 묶인 액션은 버튼도 네 개 두고 방향을 각각 고른다.
    [RequireComponent(typeof(Button))]
    public class KeyRebindButton : MonoBehaviour
    {
        // 이동 액션 안에서 어느 방향 키인지. 이동이 아닌 액션은 None으로 둔다.
        public enum MovePart
        {
            None,
            Up,
            Down,
            Left,
            Right,
        }

        [SerializeField]
        private InputActionType _actionType;

        [SerializeField]
        private MovePart _movePart;

        // 액션이 들어있는 에셋. DataContainer에 등록해두면 주입된다.
        [Preserve, Inject(true)] private InputActionAsset _actionAsset;

        [Preserve, Inject(true)] private IInputKeySettings _keySettings;

        private TMP_Text _label;

        private void Awake()
        {
            _label = GetComponentInChildren<TMP_Text>();
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void OnEnable()
        {
            ShowCurrentKey();
        }

        private void HandleClicked()
        {
            InputAction action = _actionAsset.FindAction(_actionType.ToString());

            if (action == null)
            {
                return;
            }

            _label.text = "...";

            // 바꾸는 동안은 액션을 꺼둬야 한다. 안 그러면 새로 누른 키가 게임 동작으로도 먹힌다.
            action.Disable();

            // 좌클릭만 막는다. 좌클릭이 배정되면 버튼 누르기랑 겹쳐서 설정을 빠져나올 수 없다.
            // 우클릭은 이미 대시가 쓰고 있어서 마우스를 통째로 막으면 안 된다.
            action.PerformInteractiveRebinding(FindBindingIndex(action))
                .WithControlsExcluding("<Mouse>/leftButton")
                .OnComplete(HandleRebindDone)
                .OnCancel(HandleRebindDone)
                .Start();
        }

        private void HandleRebindDone(InputActionRebindingExtensions.RebindingOperation operation)
        {
            InputAction action = operation.action;
            operation.Dispose();
            action.Enable();

            if (_keySettings != null)
            {
                // 바뀐 키 전체를 글자 하나로 만들어 조작키 주인에게 맡긴다.
                _keySettings.Bindings = _actionAsset.SaveBindingOverridesAsJson();
            }

            ShowCurrentKey();
        }

        private void ShowCurrentKey()
        {
            InputAction action = _actionAsset.FindAction(_actionType.ToString());

            if (action == null || _label == null)
            {
                return;
            }

            _label.text = action.GetBindingDisplayString(FindBindingIndex(action));
        }

        // 이동은 키 네 개가 한 액션 안에 방향별 조각으로 들어있어서, 그중 내 방향 조각을 찾아야 한다.
        // 방향이 None이면 조각이 없는 보통 액션이라 첫 번째 것을 쓴다.
        private int FindBindingIndex(InputAction action)
        {
            if (_movePart == MovePart.None)
            {
                return 0;
            }

            string partName = _movePart.ToString();

            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];

                // 에셋에 적힌 조각 이름은 대소문자가 제각각이라 그건 무시하고 비교한다.
                if (binding.isPartOfComposite && string.Equals(binding.name, partName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
