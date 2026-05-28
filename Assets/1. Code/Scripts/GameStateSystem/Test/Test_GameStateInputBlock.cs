using System;
using UnityEngine;

namespace Refactoring
{
    public class Test_GameStateInputBlock : MonoBehaviour, IGameStateEvent, IInjectTarget
    {
        public event Action<GameStateType> OnStateChanged;

        public Type[] InterfaceTypes => new[] { typeof(IGameStateEvent) };

        public GameStateRole Role => GameStateRole.Requester;


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
