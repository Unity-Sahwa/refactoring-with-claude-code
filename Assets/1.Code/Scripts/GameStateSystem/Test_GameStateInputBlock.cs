using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Refactoring
{
    public class Test_GameStateInputBlock : MonoBehaviour, IGameStateEvent, IInterfaceInjectable
    {
        public event Action<GameStateType> OnStateChanged;

        public GameStateRole Role => GameStateRole.Requester;

        public Dictionary<Type, List<object>> injectedImplements {get;} = new Dictionary<Type, List<object>>
        {
            { typeof(IGameStateEvent), new List<object>() }
        };
        

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) RequestInputBlockedByMenu();
            if (Input.GetKeyDown(KeyCode.Alpha2)) RequestInputUnblockedByMenu();
            if (Input.GetKeyDown(KeyCode.Alpha3)) RequestInputBlockedBySequence();
            if (Input.GetKeyDown(KeyCode.Alpha4)) RequestInputUnblockedBySequence();
        }

        [ContextMenu("RequestInputBlockedByMenu")]
        public void RequestInputBlockedByMenu()   => OnStateChanged?.Invoke(GameStateType.InputBlockedByMenu);
        
        [ContextMenu("RequestInputUnblockedByMenu")]
        public void RequestInputUnblockedByMenu()   => OnStateChanged?.Invoke(GameStateType.InputUnblockedByMenu);
        
        [ContextMenu("RequestInputBlockedBySequence")]
        public void RequestInputBlockedBySequence()   => OnStateChanged?.Invoke(GameStateType.InputBlockedBySequence);
        
        [ContextMenu("RequestInputUnblockedBySequence")]
        public void RequestInputUnblockedBySequence()   => OnStateChanged?.Invoke(GameStateType.InputUnblockedBySequence);
    }
}
