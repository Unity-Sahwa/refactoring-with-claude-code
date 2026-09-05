namespace Refactoring
{
    public enum StateEventCategory
    {
        // 선입력 저장 구간(저장 후 구간 끝에 발사)
        InputBuffer,
        // 입력 차단 구간(무시)
        InputBlock,
        SkillMove,
        MoveControl,
        RotateControl,
        Effect,
        Hitbox,
        Audio,
        // 스킬 카메라 셰이크 시작점
        CameraShake,
        // 스킬 카메라 줌(거리배율) 시작점
        CameraZoom,
        // 카메라 마우스 회전 입력을 막는 구간
        CameraLock,
        // 슈퍼아머 구간(피격 경직 무시)
        SuperArmor,
        // 무적 구간(피격 자체 무시)
        Invincible,
        // 처형: 타이밍에 스턴/처형 실행
        Finish,
        ObjectToggle   // 씬 오브젝트 활성화/비활성화 구간
    }
}