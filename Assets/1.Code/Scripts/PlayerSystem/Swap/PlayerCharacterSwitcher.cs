using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class PlayerCharacterSwitcher : MonoBehaviour, ICharacterSwappable, ICharacterSwapNotifier
    {
        [Inject(true)] private List<PlayerCharacter> characters;
        public PlayerCharacterType CurrentCharacter {get; private set;}
        public GameObject CurrentCharacterObject =>
            characterMap.TryGetValue(CurrentCharacter, out var c) ? c.gameObject : null;
        public event Action<PlayerCharacterType> OnCharacterSwapped;
        private Dictionary<PlayerCharacterType, PlayerCharacter> characterMap = new Dictionary<PlayerCharacterType, PlayerCharacter>();

        void Awake()
        {
            foreach (var character in characters)
            {
                characterMap[character.Type] = character;
            }
        }

        void Start()
        {
            SwapPlayerCharacter(PlayerCharacterType.HumanCharacter);
        }


        public void SwapPlayerCharacter(PlayerCharacterType type)
        {
            Vector3 currentPosition = characterMap[CurrentCharacter].transform.position;
            Quaternion currentRotation = characterMap[CurrentCharacter].transform.rotation;

            var next = characterMap[type];
            next.gameObject.SetActive(true);
            CurrentCharacter = type;
            next.transform.position = currentPosition;
            next.transform.rotation = currentRotation;

            OnCharacterSwapped?.Invoke(type);

            foreach (var (key,value) in characterMap)
            {
                if (key != type)
                {
                    value.gameObject.SetActive(false);
                }
            }
        }
    }
}