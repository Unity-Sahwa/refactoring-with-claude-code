using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    //역할: (게임에서는 2가지 캐릭터만 사용) 2개의 캐릭터를 스왑한다.
    public class PlayerCharacterSwitcher : MonoBehaviour, ICharacterSwappable, ICharacterSwapNotifier, ICurrentCharacterProvider
    {
        [Inject] private List<PlayerCharacter> _characters;
        public PlayerCharacter CurrentCharacter => _currentCharacter;
        public event Action OnCharacterSwapped;
        private PlayerCharacter _currentCharacter = null;
    

        void Awake()
        {
            foreach (var character in _characters)
            {
                if (character.Type == PlayerCharacterType.HumanCharacter)
                {
                    _currentCharacter = character;
                    break;
                }
            }

            if (_currentCharacter == null && _characters.Count > 0)
            {
                _currentCharacter = _characters[0];
            }

            foreach (var character in _characters)
            {
                character.gameObject.SetActive(character == _currentCharacter);
            }
        }

        public void SwapPlayerCharacter( )
        {
            PlayerCharacter nextCharacter = null;

            foreach (var character in _characters)
            {
                if(character.Type != _currentCharacter.Type)
                {
                    nextCharacter = character;
                    break;
                }
            }

            if (nextCharacter == null)
            {
                Debug.LogWarning("SwapPlayerCharacter: 다음캐릭터가 존재하지 않습니다.");
                return;
            }

            nextCharacter.transform.position = _currentCharacter.transform.position;
            nextCharacter.transform.rotation = _currentCharacter.transform.rotation;
            
            _currentCharacter.gameObject.SetActive(false);
            nextCharacter.gameObject.SetActive(true);
            _currentCharacter = nextCharacter;

            OnCharacterSwapped?.Invoke();
        }
    }
}