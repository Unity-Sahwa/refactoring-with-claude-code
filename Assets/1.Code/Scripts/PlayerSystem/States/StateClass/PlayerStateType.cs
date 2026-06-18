namespace Refactoring
{
    // 캐릭터 중립 상태 종류. 캐릭터(인간/동물)를 구분하지 않는다.
    // 락온 이동은 별도 상태가 아니라 LocomotionState가 IsLockOn으로 blend 좌표만 분기해 처리한다.
    // 락온 변형은 LockOnDash만 데이터(SO)로 표현. Locomotion은 정지~이동 통합 루프 상태(기존 Idle+Move).
    public enum PlayerStateType
    {
        Locomotion,

        Dash,
        LockOnDash,

        NormalAttack1,
        NormalAttack2,
        NormalAttack3,

        SpecialSkill,
        ExecutionSkill,

        Jump,
        Hit,
        Dead,
    }
}
