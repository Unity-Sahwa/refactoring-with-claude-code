namespace Refactoring
{
    // ID(소리가 쓰이는 곳) 이름표.
    // 같은 클립이라도 쓰임이 다르면 다른 id를 만든다(예: 약공격 타격음 / 강공격 타격음).
    public enum AudioId
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
    }
}
