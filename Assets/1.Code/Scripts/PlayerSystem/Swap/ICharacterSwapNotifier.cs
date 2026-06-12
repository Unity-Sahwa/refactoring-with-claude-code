using System;
using UnityEngine;

namespace Refactoring
{
    public interface ICharacterSwapNotifier
    {
        public PlayerCharacterType CurrentCharacter {get;}
        public GameObject CurrentCharacterObject {get;}   // 현재 캐릭터의 실체. 공격자 정보 등에 쓴다.
        public event Action<PlayerCharacterType> OnCharacterSwapped;
    }
}