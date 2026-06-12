using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class Test_PlayerCharacterSwitcher : MonoBehaviour, IInterfaceInjectable
    {
        public Dictionary<Type, List<object>> injectedImplements {get;} = new Dictionary<Type, List<object>>()
        {
            {typeof(IInputEventProvider), new List<object>()},
            {typeof(ICharacterSwappable), new List<object>()}
        };

        private IInputEventProvider _inputEvent;
        private ICharacterSwappable _characterSwitcher;
        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 20;
            _inputEvent = (IInputEventProvider)injectedImplements[typeof(IInputEventProvider)][0];
            _inputEvent.OnInputPressed += OnTest;

            _characterSwitcher = (ICharacterSwappable)injectedImplements[typeof(ICharacterSwappable)][0];
        }
        void OnTest(InputActionType type)
        {
            if(type == InputActionType.Menu)
            {
                if(_characterSwitcher.CurrentCharacter == PlayerCharacterType.HumanCharacter)
                {
                    _characterSwitcher.SwapPlayerCharacter(PlayerCharacterType.AnimalCharacter);
                }
                else if(_characterSwitcher.CurrentCharacter == PlayerCharacterType.AnimalCharacter)
                {
                    _characterSwitcher.SwapPlayerCharacter(PlayerCharacterType.HumanCharacter);
                }
            }
        }
    }
}

