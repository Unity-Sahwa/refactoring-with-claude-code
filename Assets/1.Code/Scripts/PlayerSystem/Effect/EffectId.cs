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
        HumanSpecialAttack,
        HumanFinishAttack,

        // 동물탈
        AnimalNormalAttack1,
        AnimalNormalAttack2,
        AnimalNormalAttack3,
        AnimalSpecialAttack,
        AnimalFinishAttack,
    }
}
