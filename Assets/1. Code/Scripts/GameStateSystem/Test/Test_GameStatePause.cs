using System;
using UnityEngine;

namespace Refactoring
{
    public class Test_GameStatePause : MonoBehaviour, IGameStateEvent
    {
        public GameStateRole Role => GameStateRole.Requester;
        public event Action<GameStateType> OnStateChanged;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha5)) RequestPause();
            if (Input.GetKeyDown(KeyCode.Alpha6)) RequestResume();
            if (Input.GetKeyDown(KeyCode.Alpha7)) RequestRestart();
            if (Input.GetKeyDown(KeyCode.Alpha8)) RequestGameOver();
        }

        [ContextMenu("Request Pause")]
        public void RequestPause()  => OnStateChanged?.Invoke(GameStateType.Paused);

        
        [ContextMenu("Request Resume")]
        public void RequestResume() => OnStateChanged?.Invoke(GameStateType.Resumed);

        public void RequestRestart() => OnStateChanged?.Invoke(GameStateType.Restarted);
        public void RequestGameOver() => OnStateChanged?.Invoke(GameStateType.GameOver);
    }
}
