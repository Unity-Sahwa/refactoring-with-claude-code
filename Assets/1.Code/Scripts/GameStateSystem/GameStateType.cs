using System;

namespace Refactoring
{
    public enum GameStateType 
    {
        GamePlay,  // 사용자 인게임 플레이(default)
        Cutscene,  // 컷씬: 플레이가 멈추고 영상처럼 재생되는 구간
        Menu       // 메뉴 UI
    }
}
