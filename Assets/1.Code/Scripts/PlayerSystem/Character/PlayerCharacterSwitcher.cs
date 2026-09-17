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
            if (_characters == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerCharacterSwitcher)}: 필수 의존 주입 실패");
            }

            _currentCharacter = FindInitialCharacter();
            ActivateOnlyCurrent();
        }

        private PlayerCharacter FindInitialCharacter()
        {
            foreach (var character in _characters)
            {
                if (character.Type == PlayerCharacterType.HumanCharacter)
                {
                    return character;
                }
            }

            return _characters.Count > 0 ? _characters[0] : null;
        }

        private void ActivateOnlyCurrent()
        {
            foreach (var character in _characters)
            {
                character.gameObject.SetActive(character == _currentCharacter);
            }
        }

        public void SwapPlayerCharacter()
        {
            PlayerCharacter nextCharacter = FindNextCharacter();
            if (nextCharacter == null)
            {
                Debug.LogWarning("SwapPlayerCharacter: 다음캐릭터가 존재하지 않습니다.");
                return;
            }

            MoveToCurrentTransform(nextCharacter);
            SwitchActiveCharacter(nextCharacter);

            OnCharacterSwapped?.Invoke();
        }

        private PlayerCharacter FindNextCharacter()
        {
            foreach (var character in _characters)
            {
                if (character.Type != _currentCharacter.Type)
                {
                    return character;
                }
            }

            return null;
        }

        private void MoveToCurrentTransform(PlayerCharacter nextCharacter)
        {
            nextCharacter.transform.position = _currentCharacter.transform.position;
            nextCharacter.transform.rotation = _currentCharacter.transform.rotation;
        }

        private void SwitchActiveCharacter(PlayerCharacter nextCharacter)
        {
            _currentCharacter.gameObject.SetActive(false);
            nextCharacter.gameObject.SetActive(true);
            _currentCharacter = nextCharacter;
        }
    }
}