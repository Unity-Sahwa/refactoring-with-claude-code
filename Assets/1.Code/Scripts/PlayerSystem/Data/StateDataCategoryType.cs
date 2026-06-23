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
        SuperArmor,    // 슈퍼아머 구간(피격 경직 무시)
        Invincible     // 무적 구간(피격 자체 무시)
    }
}