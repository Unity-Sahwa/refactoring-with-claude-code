namespace Refactoring
{
    // 이펙트 id 이름표. 카탈로그(EffectCatalog)에서 이 id에 프리팹을 붙인다.
    // 필요에 맞게 추가/삭제한다.(AudioId와 같은 방식)
    public enum EffectId
    {
        None = 0,

        // 사람탈
        HumanNormalAttack1,
        HumanNormalAttack2,
        HumanNormalAttack3,
        HumanSpecialAttackSwing,
        HumanSpecialAttackSplash,
        

        // 동물탈
        AnimalNormalAttack1,
        AnimalNormalAttack2,
        AnimalNormalAttack3,
        AnimalSpecialAttackFloat,
        AnimalSpecialAttackSlash,


        // 처형 공통
        FinishAttackDome,
        FinishAttackSlash,

        // 화면 연출
        HitVignette,   // 피격 시 화면 가장자리 비네트 깜빡임
        SwapToAnimalMask,
        SwapToHumanMask,
        SwapToGhostMask
    }
}
