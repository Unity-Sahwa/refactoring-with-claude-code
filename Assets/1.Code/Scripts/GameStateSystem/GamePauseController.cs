using UnityEngine;

namespace Refactoring
{
    // 게임 모드가 Menu가 되면 시간을 멈추고, 벗어나면 되돌린다. UI와 분리된 단일 책임.
    public class GamePauseController : MonoBehaviour
    {
        [Inject] private IGameStateProvider _gameState;

        private void Awake()
        {
            if (_gameState != null)
            {
                _gameState.OnChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (_gameState != null)
            {
                _gameState.OnChanged -= HandleStateChanged;
            }

            Time.timeScale = 1f;   // 정지 중 파괴돼도 시간이 멈춘 채 남지 않게
        }

        // Menu 모드에선 정지(0), 그 외에는 정상 속도(1).
        private void HandleStateChanged(GameStateType state)
        {
            Time.timeScale = state == GameStateType.Menu ? 0f : 1f;
        }
    }
}
