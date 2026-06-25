using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class PlayerCharacterSwitcher : MonoBehaviour, ICharacterSwappable, ICharacterSwapNotifier
    {
        [Inject] private List<PlayerCharacter> characters;
        public PlayerCharacterType CurrentCharacterType => currentCharacter.Type;
        public GameObject CurrentCharacterObject => currentCharacter.gameObject;
        public event Action<PlayerCharacterType> OnCharacterSwapped;
        private PlayerCharacter currentCharacter = null;
    

        void Awake()
        {
            foreach (var character in characters)
            {
                if (character.Type == PlayerCharacterType.HumanCharacter)
                {
                    currentCharacter = character;
                    break;
                }
            }

            if (currentCharacter == null && characters.Count > 0)
            {
                currentCharacter = characters[0];
            }

            foreach (var character in characters)
            {
                character.gameObject.SetActive(character == currentCharacter);
            }
        }

        public void SwapPlayerCharacter( )
        {
            
            PlayerCharacter nextCharacter = null;

            foreach (var character in characters)
            {
                if(character.Type != currentCharacter.Type)
                {
                    nextCharacter = character;
                    break;
                }
            }

            nextCharacter.transform.position = currentCharacter.transform.position;
            nextCharacter.transform.rotation = currentCharacter.transform.rotation;
            
            currentCharacter.gameObject.SetActive(false);
            nextCharacter.gameObject.SetActive(true);
            currentCharacter = nextCharacter;

            OnCharacterSwapped?.Invoke(CurrentCharacterType);
        }
    }
}