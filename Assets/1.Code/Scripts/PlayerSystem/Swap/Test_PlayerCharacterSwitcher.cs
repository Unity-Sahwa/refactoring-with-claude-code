using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class Test_PlayerCharacterSwitcher : MonoBehaviour
    {
        [Inject] private IInputEventProvider _inputEvent;
        [Inject] private ICharacterSwappable _characterSwitcher;
        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            
            _inputEvent.OnInputPressed += OnTest;
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

