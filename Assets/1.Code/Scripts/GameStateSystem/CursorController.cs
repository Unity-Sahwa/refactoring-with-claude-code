using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 게임 모드에 따라 커서를 제어한다. Menu면 표시·잠금 해제, 그 외엔 숨김·중앙 잠금.
    public class CursorController : MonoBehaviour
    {
        [Preserve , Inject] private IGameStateProvider _gameState;

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
            bool inMenu = state == GameStateType.Menu;
            Cursor.visible = inMenu;
            Cursor.lockState = inMenu ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
