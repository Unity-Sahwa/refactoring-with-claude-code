using UnityEngine;


namespace Refactoring
{
    public class HumanCharacter : BaseCharacter<PlayerCharacterType>
    {
        public override PlayerCharacterType Type => PlayerCharacterType.HumanCharacter;
    }
}