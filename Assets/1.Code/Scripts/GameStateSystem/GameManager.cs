using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 게임 전체 흐름 관리 — Requester 이벤트를 중계하고 외부에 재발사
    public class GameManager : MonoBehaviour, IGameStateEvent, IInputBlocker, IInjectTarget, IInjectRequester
    {
        public GameStateRole Role => GameStateRole.Manager; // IGameStateEvent
        public event Action<GameStateType> OnStateChanged; // IGameStateEvent

        private bool isInputBlockedByMenu = false; // 메뉴(UI)에 의한 입력 차단 여부
        private bool isInputBlockedBySequence = false; // 연출(시퀀스)에 의한 입력 차단 여부
        public bool IsInputBlocked => isInputBlockedByMenu || isInputBlockedBySequence; // 둘 중 하나라도 true면 입력 전체 차단
        public bool IsPaused { get; private set; }

        public Type[] InterfaceTypes => new[] { typeof(IGameStateEvent), typeof(IInputBlocker) }; //IInjectTarget
        public List<Type> TargetTypes => new List<Type> { typeof(IGameStateEvent) }; //IInjectRequester

        public void Inject(Dictionary<Type, List<object>> targets) //IInjectRequester
        {
            if (targets.TryGetValue(typeof(IGameStateEvent), out var list))
            {
                foreach (var obj in list)
                {
                    if (obj is IGameStateEvent e && e.Role == GameStateRole.Requester)
                    {
                        e.OnStateChanged += HandleRequesterState;
                    }
                }
            }
        }

        public void BlockInputByMenu(bool isBlocked)
        {
            isInputBlockedByMenu = isBlocked;
            OnStateChanged?.Invoke(isBlocked ? GameStateType.InputBlockedByMenu : GameStateType.InputUnblockedByMenu);
        }
        public void BlockInputBySequence(bool isBlocked)
        {
            isInputBlockedBySequence = isBlocked;
            OnStateChanged?.Invoke(isBlocked ? GameStateType.InputBlockedBySequence : GameStateType.InputUnblockedBySequence);
        }

        public void GamePause()
        {
            IsPaused = true;
            OnStateChanged?.Invoke(GameStateType.Paused);
        }
        public void GameResume()
        {
            IsPaused = false;
            OnStateChanged?.Invoke(GameStateType.Resumed);
        }
        public void GameRestart()
        {
            OnStateChanged?.Invoke(GameStateType.Restarted);
        }
        public void GameOver()
        {
            isInputBlockedByMenu = true;
            isInputBlockedBySequence = true;
            OnStateChanged?.Invoke(GameStateType.GameOver);
        }

        private void HandleRequesterState(GameStateType type)
        {
            switch (type)
            {
                case GameStateType.Paused:                      GamePause();                    break;
                case GameStateType.Resumed:                     GameResume();                   break;
                case GameStateType.InputBlockedByMenu:          BlockInputByMenu(true);         break;
                case GameStateType.InputUnblockedByMenu:        BlockInputByMenu(false);        break;
                case GameStateType.InputBlockedBySequence:      BlockInputBySequence(true);     break;
                case GameStateType.InputUnblockedBySequence:    BlockInputBySequence(false);    break;
                case GameStateType.Restarted:                   GameRestart();                  break;    
                case GameStateType.GameOver:                    GameOver();                     break;
            }
        }
    }
}
