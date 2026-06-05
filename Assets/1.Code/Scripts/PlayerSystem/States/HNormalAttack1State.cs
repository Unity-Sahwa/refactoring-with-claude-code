using UnityEngine;

namespace Refactoring
{
    public class HNormalAttack1State : CharacterBaseState<PlayerStateType, PlayerCharacterType>
    {
        public override PlayerStateType StateKey => PlayerStateType.HNormalAttack1;
        public override bool CanReenter => false;
        protected override string AnimationName => "HNormalAttack1";
        public override PlayerCharacterType CharacterType => PlayerCharacterType.HumanCharacter;
    }
}
