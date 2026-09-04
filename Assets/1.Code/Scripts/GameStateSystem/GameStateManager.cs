using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 게임 모드 스택을 보유하고 현재 모드를 알린다.
    // 흐름: Push 요청 → 우선순위 비교 → 통과하면 스택에 얹고 OnChanged 발행 (GamePlay는 바닥 고정)
    public class GameStateManager : MonoBehaviour, IGameStateProvider, IGameStateController
    {
        // 바닥에 항상 게임플레이가 존재
        private readonly Stack<GameStateType> _stack = new(new[] { GameStateType.GamePlay });

        public GameStateType Current => _stack.Peek();
        public event Action<GameStateType> OnChanged;

        // 게임 상태를 교체하여, 불필요한 입력이 들어가는 걸 방지함.
        public void Push(GameStateType state)
        {
            if (state == GameStateType.GamePlay || Priority(state) <= Priority(Current))
            {
                // 얹히지 않은 걸 모르면 입력이 왜 안 막히는지 추적할 수 없다.
                Debug.LogWarning($"{nameof(GameStateManager)}: {state} Push 무시됨(현재 {Current})");
                return;
            }

            _stack.Push(state);
            OnChanged?.Invoke(Current);
        }

        // 다른 상태가 빠지는 것을 방지하기 위해 매개변수 추가
        public void Pop(GameStateType state)
        {
            if (state == GameStateType.GamePlay || _stack.Count <= 1 || !Current.Equals(state))
            {
                // 남의 상태를 내리려 한 경우다. 조용히 넘기면 모드가 갇힌 채로 남는다.
                Debug.LogWarning($"{nameof(GameStateManager)}: {state} Pop 무시됨(현재 {Current})");
                return;
            }

            _stack.Pop();
            OnChanged?.Invoke(Current);
        }

        // 메뉴 > 컷씬 > 게임 플레이 순으로 우선순위를 나눔
        private int Priority(GameStateType state)
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
