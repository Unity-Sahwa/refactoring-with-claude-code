using System;
using UnityEngine;

namespace Refactoring
{
    public interface ICharacterSwappable
    {
        public PlayerCharacterType CurrentCharacterType {get;}
        public void SwapPlayerCharacter();
    }
}