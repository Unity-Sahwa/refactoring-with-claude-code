namespace Refactoring
{
    //대원TODO: 다음 프로젝트에서는 Enum에 index를 카테고리별로 매겨야할 듯. 아니면 Enum 밀리는 문제를 다르게 해결하던가
    public enum SoundType
    {
        None = 0,

        // 사람탈
        HumanNormalAttack1,
        HumanNormalAttack1Hit,
        HumanNormalAttack2,
        HumanNormalAttack2Hit,
        HumanNormalAttack3,
        HumanNormalAttack3Hit,
        HumanSpecialAttackSpin,
        HumanSpecialAttackSplash,
        HumanSpecialAttackHit,
        HumanFinishAttackHitGround,
        HumanFinishAttackSwing,
        HumanFinishAttackAfterSwing,
        HumanFinishAttackHit,
        HumanBackDash,
        HumanWalk,


        // 동물탈
        AnimalNormalAttack1,
        AnimalNormalAttack1Hit,
        AnimalNormalAttack2,
        AnimalNormalAttack2Hit,
        AnimalNormalAttack3,
        AnimalNormalAttack3Hit,
        AnimalSpecialAttackJump,
        AnimalSpecialAttackFloat,
        AnimalSpecialAttackSlash,
        AnimalSpecialAttackHit,
        AnimalFinishAttackSweap,
        AnimalFinishAttackSwing,
        AnimalFinishAttackAfterSwing,
        AnimalFinishAttackHit,
        AnimalBackDash,
        AnimalWalk,

        // 플레이어 공용
        FrontDash,
        Hit,
        HitHeartBeat,
        Die,
        CharacterSwap,


        // UI
        UIClick,
        UIHover,

        // 파트너
        PlayerPartner,
    }
}
