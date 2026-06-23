using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 게임 모드 스택을 보유하고 현재 모드를 알린다. 
    // 규칙: 현재 top보다 우선순위가 높은 모드만 위에 얹는다. GamePlay는 바닥 고정(직접 Push/Pop 불가).
    public class GameStateManager : MonoBehaviour, IGameStateProvider, IGameStateController
    {
        private readonly Stack<GameStateType> _stack = new(new[] { GameStateType.GamePlay }); //바닥에 항상 게임플레이가 존재

        public GameStateType Current => _stack.Peek();
        public event Action<GameStateType> OnChanged;

        public void Push(GameStateType state)
        //게임 상태를 교체하여, 불필요한 입력이 들어가는 걸 방지함.
        {
            if (state == GameStateType.GamePlay || Priority(state) <= Priority(Current)) 
            {
                return;
            }

            _stack.Push(state);
            OnChanged?.Invoke(Current);
        }

        public void Pop(GameStateType state) 
        //다른 상태가 빠지는 것을 방지하기 위해 매개변수 추가
        {
            if (state == GameStateType.GamePlay || _stack.Count <= 1 || !Current.Equals(state)) 
            {
                return;
            }

            _stack.Pop();
            OnChanged?.Invoke(Current);
        }

        private int Priority(GameStateType state) 
        //메뉴 > 컷씬 > 게임 플레이 순으로 우선순위를 나눔
        {
            switch (state)
            {
                case GameStateType.Menu:     return 2;
                case GameStateType.Cutscene: return 1;
                default:                     return 0; // GamePlay
            }
        }
    }
}
