using System;

namespace Refactoring
{
    public interface ICharacterSwapNotifier
    {
        public PlayerCharacterType CurrentCharacter {get;}
        public event Action<PlayerCharacterType> OnCharacterSwapped;
    }
}