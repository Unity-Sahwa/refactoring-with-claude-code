namespace Refactoring
{
    // 씬에서 켜고 끌 대상 구분용 key. 필요한 대상이 생길 때마다 여기에 추가한다.
    public enum ToggleTargetKey
    {
        None = 0,
        GhostMaskForAnimal = 100,
        GhostWeaponForAnimal,
        AnimalMask,
        AnimalWeapon1,
        AnimalWeapon2,
        GhostMaskEffectForAniaml,

        GhostMaskForHuman = 200, 
        GhostWeaponForHuman,
        HumanMask,
        HumanWeapon,
        GhostMaskEffectForHuman,
    }
}
