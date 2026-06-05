using UnityEngine;

namespace Refactoring
{
    public class HNormalAttack2State : CharacterBaseState<PlayerStateType, PlayerCharacterType>
    {
        public override PlayerStateType StateKey => PlayerStateType.HNormalAttack2;
        public override bool CanReenter => false;
        protected override string AnimationName => "HNormalAttack2";
        public override PlayerCharacterType CharacterType => PlayerCharacterType.HumanCharacter;
    }
}
