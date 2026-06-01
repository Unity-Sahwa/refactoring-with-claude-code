using System;
using UnityEngine;

namespace Refactoring
{
    public interface ICharacterSwappable
    {
        public PlayerCharacterType CurrentCharacter {get;}
        public void SwapPlayerCharacter(PlayerCharacterType type);
    }
}