using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 두 캐릭터를 서로 바꾸고, 바뀐 사실을 알린다.
    public class PlayerCharacterSwitcher : MonoBehaviour, ICharacterSwappable, ICharacterSwapNotifier, ICurrentCharacterProvider
    {
        [Preserve, Inject] private List<PlayerCharacter> _characters;

        private PlayerCharacter _currentCharacter;

        public PlayerCharacterType? CurrentType => _currentCharacter != null ? _currentCharacter.Type : null;

        public T GetCurrentComponent<T>() where T : Component
        {
            return _currentCharacter != null ? _currentCharacter.GetCharacterComponent<T>() : null;
        }

        public event Action OnCharacterSwapped;

        private void Awake()
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

        public void SwapPlayerCharacter()
        {
            PlayerCharacter nextCharacter = null;

            foreach (var character in _characters)
            {
                if (character.Type != _currentCharacter.Type)
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