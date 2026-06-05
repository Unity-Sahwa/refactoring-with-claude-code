using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class PlayerCharacterSwitcher : MonoBehaviour, 
                    IInterfaceInjectable, ICharacterSwappable, ICharacterSwapNotifier
    {
        public Dictionary<Type, List<object>> injectedImplements {get;} = new Dictionary<Type, List<object>>()
        {
            {typeof(BaseCharacter<PlayerCharacterType>), new List<object>()}
        };
        public PlayerCharacterType CurrentCharacter {get; private set;}
        public event Action<PlayerCharacterType> OnCharacterSwapped;
        private Dictionary<PlayerCharacterType, BaseCharacter<PlayerCharacterType>> characterMap = new Dictionary<PlayerCharacterType, BaseCharacter<PlayerCharacterType>>();

        void Awake()
        {
            var characterList = injectedImplements[typeof(BaseCharacter<PlayerCharacterType>)];
            foreach (var obj in characterList)
            {
                BaseCharacter<PlayerCharacterType> character = (BaseCharacter<PlayerCharacterType>)obj;
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

            foreach (var (key,value) in characterMap)
            {
                if (key == type)
                {
                    value.gameObject.SetActive(true);

                    CurrentCharacter = type;
                    characterMap[type].transform.position = currentPosition;
                    characterMap[type].transform.rotation = currentRotation;
                }
                else
                {
                    value.gameObject.SetActive(false);
                }
            }

            OnCharacterSwapped?.Invoke(type);
        }
    }
}