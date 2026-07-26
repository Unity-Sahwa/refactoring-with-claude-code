namespace Refactoring
{
    // 역할: UI 효과 대상을 식별하는 아이디. 외부는 이 값만 넘겨서 효과를 요청함.
    public enum UIEffectId
    {
        None = 0,
        HealthHudShake,  // 체력 HUD 흔들기
        HealthHudFade,   // 체력 HUD 페이드
    }
}
