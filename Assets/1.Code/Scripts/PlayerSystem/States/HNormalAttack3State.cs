using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class HNormalAttack3State : CharacterBaseState<PlayerStateType, PlayerCharacterType>
    {
        public override PlayerStateType StateKey => PlayerStateType.HNormalAttack3;
        public override bool CanReenter => false;
        protected override string AnimationName => "HNormalAttack3";
        public override PlayerCharacterType CharacterType => PlayerCharacterType.HumanCharacter;
    }
}
