namespace Refactoring
{
    public enum StateEventCategory
    {
        InputBuffer,   // 선입력 저장 구간(저장 후 구간 끝에 발사)
        InputBlock,    // 입력 차단 구간(무시)
        SkillMove,
        MoveControl,
        RotateControl,
        Effect,
        Hitbox,
        Audio,
        CameraShake,   // 스킬 카메라 셰이크 시작점
        CameraZoom,    // 스킬 카메라 줌(거리배율) 시작점
        CameraLock,    // 카메라 마우스 회전 입력을 막는 구간
        SuperArmor,    // 슈퍼아머 구간(피격 경직 무시)
        Invincible,    // 무적 구간(피격 자체 무시)
        Finish,        // 처형: 타이밍에 스턴/처형 실행
        ObjectToggle   // 씬 오브젝트 활성화/비활성화 구간
    }
}