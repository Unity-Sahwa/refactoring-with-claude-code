using UnityEngine;

namespace Refactoring
{
    // 컷씬 상태로 바꿔 플레이어 조작을 막거나, 다시 풀어준다.
    // 막을 곳과 풀 곳에 각각 하나씩 붙여서 쓴다.
    public class EventCutsceneLock : EventData
    {
        [Tooltip("켜면 조작을 막고, 끄면 풀어준다")]
        [SerializeField]
        private bool _isLock = true;

        [Inject(true)]
        private IGameStateController _gameState;

        public override void Execute()
        {
            if (_isLock)
            {
                _gameState?.Push(GameStateType.Cutscene);
            }
            else
            {
                _gameState?.Pop(GameStateType.Cutscene);
            }
        }
    }
}
