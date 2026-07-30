namespace Refactoring
{
    // 창 종류를 구분하는 이름표. 창을 글자가 아니라 이 값으로 지목한다.
    // 순서를 바꾸면 씬에 이미 골라둔 값이 밀리므로, 새 항목은 항상 맨 아래에 붙인다.
    public enum WindowId
    {
        MainMenu,
        Pause,
        NewGameConfirm,
        LoadSlots,
        LoadConfirm,
        Settings,
        SettingsGameplay,
        SettingsSound,
        SettingsLanguage,
        QuitConfirm,
        Credit,
        StoryToon,
        SettingsPlatform,
        Confirm,
        // 정지메뉴에서 "메뉴로 나갈까요?" 물어보는 창.
        ExitToMenuConfirm,
    }
}
