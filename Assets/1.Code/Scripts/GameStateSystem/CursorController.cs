using UnityEngine;

namespace Refactoring
{
    // 게임 모드에 따라 커서를 제어한다. Menu면 표시·잠금 해제, 그 외엔 숨김·중앙 잠금.
    public class CursorController : MonoBehaviour
    {
        // 에디터에서 모바일용 화면을 테스트할 때만 켬. 커서를 계속 보이게 두고 잠그지도 않음.
        // 실제 모바일 기기는 커서 자체가 없어서 이 값과 무관함.
        [SerializeField] private bool isMobileTest = false;
        [Inject] private IGameStateProvider _gameState;

        private void Start()
        {
            if (_gameState != null)
            {
                _gameState.OnChanged += HandleStateChanged;
            }

            // OnChanged는 변경 시에만 오므로, 시작 시 현재 모드 기준으로 한 번 맞춘다.
            Apply(_gameState != null ? _gameState.Current : GameStateType.GamePlay);
        }

        private void OnDestroy()
        {
            if (_gameState != null)
            {
                _gameState.OnChanged -= HandleStateChanged;
            }

            // 씬을 떠날 때 커서를 표시 상태로 되돌린다(다음 씬이 커서를 안 만져도 보이도록).
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void HandleStateChanged(GameStateType state) => Apply(state);

        private void Apply(GameStateType state)
        {
            if (isMobileTest)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                return;
            }

            bool inMenu = state == GameStateType.Menu;
            Cursor.visible = inMenu;
            Cursor.lockState = inMenu ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
