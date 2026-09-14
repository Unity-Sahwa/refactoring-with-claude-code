using System;

namespace Refactoring
{
    // 책임: 지금 컷씬인지와 그 변화를 읽기 전용으로 알려준다. 구독자는 다른 상태(Menu 등)는 몰라도 된다.
    public interface ICutsceneStateProvider
    {
        bool IsCutscene { get; }
        event Action OnCutsceneChanged;
    }
}
