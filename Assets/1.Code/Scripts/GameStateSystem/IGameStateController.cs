using System;

namespace Refactoring
{
    // 책임: 조작용 — 메뉴·컷씬 측이 모드를 얹고(Push) 내린다(Pop). 언제 호출할지는 호출자가 판단.
    public interface IGameStateController
    {
        void Push(GameStateType state);
        void Pop(GameStateType state);
    }
}
