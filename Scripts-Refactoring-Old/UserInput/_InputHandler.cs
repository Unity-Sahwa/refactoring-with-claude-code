using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(_UserInput))]
public class _InputHandler : MonoBehaviour
{
    //public delegate void OnPlayerInputUpdated(KeyActionEnum inputkeys);
    //public event OnPlayerInputUpdated OnPlayerInputUpdatedEvent;
    //public delegate void OnPlayerDirVectorUpdated(Dictionary<KeyActionEnum, int> dirValue, Vector3 dirVector3);
    //public event OnPlayerDirVectorUpdated OnPlayerDirVectorUpdatedEvent;

    private bool isInputBlockedByMenu = false;
    private bool isInputBlockedByCutScene = false;

    public void HandleInput(InputActionEnum inputkeys)
    {
        if (isInputBlockedByMenu || isInputBlockedByCutScene) return;
        
        if (inputkeys == InputActionEnum.Menu)
        {

        }
        else
        {
            //OnPlayerInputUpdatedEvent.Invoke(inputkeys);
        }
    }

    
}
