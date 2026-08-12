using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 메뉴 UI 총괄(프런트 데스크). 자식 창들을 이름표로 관리하고, 열기/닫기와 게임 모드 전환을 조율한다.
    // 인게임: z(Menu)로 열고 닫으며 모드 전환. 메인메뉴: 시작 시 자동으로 열고 z로는 닫지 않음.
    public class UIRoot : MonoBehaviour
    {
        // 처음 여는 기본 창. 인게임 씬=Pause, 메인메뉴 씬=MainMenu로 인스펙터에서 지정.
        [SerializeField]
        private WindowId _entryWindow;

        // 입력·상태는 인게임에서만 필요하다(메인메뉴는 버튼만 씀). 없어도 되도록 Optional 주입.
        [Inject(true)] private IInputPressedProvider _gameplayInput;   // 게임플레이 중 입력(메뉴 열기)
        [Inject(true)] private IMenuInputProvider _menuInput;          // 메뉴 중 입력(닫기)
        [Inject(true)] private ICutsceneInputProvider _cutsceneInput;  // 컷씬 중 입력(메뉴 열기)
        [Inject(true)] private IGameStateController _gameState;        // 메뉴 진입/복귀 시 모드 전환

        private readonly Dictionary<WindowId, IWindow> _registry = new();
        private readonly UINavigator _navigator = new();

        // 기본 창이 MainMenu면 메인메뉴 씬으로 본다(시작 시 자동 열기 + z 닫기 금지).
        private bool IsMainMenuScene => _entryWindow == WindowId.MainMenu;

        private void Awake()
        {
            BuildRegistry();

            if (_gameplayInput != null)
            {
                _gameplayInput.OnInputPressed += HandleGameplayPressed;
            }

            if (_menuInput != null)
            {
                _menuInput.OnMenuPressed += HandleMenuPressed;
            }

            if (_cutsceneInput != null)
            {
                _cutsceneInput.OnCutscenePressed += HandleGameplayPressed;
            }
        }

        private void Start()
        {
            // 메인메뉴 씬은 게임플레이가 없으니, z를 기다리지 않고 시작 시 바로 메뉴를 연다.
            if (IsMainMenuScene)
            {
                _gameState?.Push(GameStateType.Menu);
                OpenWindow(_entryWindow);
            }
        }

        private void OnDestroy()
        {
            if (_gameplayInput != null)
            {
                _gameplayInput.OnInputPressed -= HandleGameplayPressed;
            }

            if (_menuInput != null)
            {
                _menuInput.OnMenuPressed -= HandleMenuPressed;
            }

            if (_cutsceneInput != null)
            {
                _cutsceneInput.OnCutscenePressed -= HandleGameplayPressed;
            }
        }

        // 자식으로 붙은 창들을 이름표 → 창으로 모은다(비활성 포함).
        private void BuildRegistry()
        {
            foreach (UIWindow window in GetComponentsInChildren<UIWindow>(true))
            {
                _registry[window.Id] = window;
            }
        }

        // 게임플레이 중 z(Menu)를 누르면: 게임 모드를 Menu로 올리고 기본 메뉴를 연다.
        private void HandleGameplayPressed(InputActionType type)
        {
            if (type != InputActionType.Menu)
            {
                return;
            }

            _gameState?.Push(GameStateType.Menu);
            OpenWindow(_entryWindow);
        }

        // 메뉴 중 z(Menu)를 누르면 맨 위 창을 한 겹 닫는다.
        private void HandleMenuPressed(InputActionType type)
        {
            if (type != InputActionType.Menu)
            {
                return;
            }

            // 메인메뉴 씬에선 메인메뉴 자체는 닫지 않는다. 그 위에 열린 창만 닫힌다.
            if (IsMainMenuScene && _navigator.Top is UIWindow top && top.Id == _entryWindow)
            {
                return;
            }

            CloseTop();
        }

        public void OpenWindow(WindowId id)
        {
            if (_registry.TryGetValue(id, out IWindow window))
            {
                _navigator.Open(window);
            }
        }

        public void CloseTop()
        {
            _navigator.CloseTop();

            // 인게임에서 열린 창이 다 사라지면 게임으로 복귀(메인메뉴 씬은 복귀 대상이 없음).
            if (!IsMainMenuScene && !_navigator.HasOpenWindow)
            {
                _gameState?.Pop(GameStateType.Menu);
            }
        }
    }
}
