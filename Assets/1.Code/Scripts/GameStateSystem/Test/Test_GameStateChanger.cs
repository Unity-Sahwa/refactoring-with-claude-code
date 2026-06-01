using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // GameManager 이벤트 구독 → 실제 게임 상태 적용 (테스트용: 화면 텍스트 출력)
    public class Test_GameStateChanger : MonoBehaviour, IInjectRequester
    {
        [SerializeField] private Text statusText;

        public List<Type> TargetTypes => new List<Type> { typeof(IGameStateEvent) };

        public void Inject(Dictionary<Type, List<object>> targets)
        {
            if (!targets.TryGetValue(typeof(IGameStateEvent), out var list)) return;

            foreach (var obj in list)
            {
                if (obj is IGameStateEvent e && e.Role == GameStateRole.Manager)
                    e.OnStateChanged += HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameStateType type)
        {
            switch (type)
            {
                case GameStateType.Paused:         ShowStatus("Paused");          break;
                case GameStateType.Resumed:        ShowStatus("Resumed");         break;
                case GameStateType.InputBlockedByMenu:   ShowStatus("InputBlockedByMenu");   break;
                case GameStateType.InputBlockedBySequence: ShowStatus("InputBlockedBySequence"); break;
                case GameStateType.InputUnblockedByMenu: ShowStatus("InputUnblockedByMenu"); break;
                case GameStateType.InputUnblockedBySequence: ShowStatus("InputUnblockedBySequence"); break;
                case GameStateType.GameOver:       ShowStatus("Game Over");       break;
            }
        }

        private void ShowStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}
