namespace Refactoring
{
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
