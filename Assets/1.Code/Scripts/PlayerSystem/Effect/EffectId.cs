namespace Refactoring
{
    // 이펙트 id 이름표. 카탈로그(EffectCatalog)에서 이 id에 프리팹을 붙인다.
    // 필요에 맞게 추가/삭제한다.(AudioId와 같은 방식)
    //대원_TODO: Enum이 에셋이나 어딘가에서 사용된다면 Enum을 추가했을 때 밀리는 문제가 발생함. 어찌해야할까
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


        //처형 공통
        FinishAttackDome,
        FinishAttackSlash,

        // 화면 연출
        HitVignette,   // 피격 시 화면 가장자리 비네트 깜빡임
        SwapToAnimalMask,
        SwapToHumanMask,
        SwapToGhostMask
    }
}
