using UnityEngine;

namespace Refactoring
{
    public class AIdleState : CharacterBaseState<PlayerStateType, PlayerCharacterType>
    {
        public override PlayerStateType StateKey => PlayerStateType.AIdle;
        public override bool CanReenter => true;
        protected override string AnimationName => "AIdle";
        public override PlayerCharacterType CharacterType => PlayerCharacterType.AnimalCharacter;
    }
}
