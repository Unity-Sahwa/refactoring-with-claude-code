// using System;
// using System.Collections.Generic;
// using UnityEngine;

// // 플레이어 캐릭터 상태 관리(전환, Update)를 담당한다.
// // FSM 구
// namespace Refactoring
// {
//     public abstract class StateManager<EState,ECharacter> : MonoBehaviour where EState : Enum where ECharacter : Enum
//     {
//         // state 인스턴스를 보관하는 딕셔너리 
//         protected Dictionary<EState, CharacterBaseState<EState,ECharacter>> states = new Dictionary<EState, CharacterBaseState<EState,ECharacter>>();

//         public EState PreviousStateKey { get; private set; }
//         public CharacterBaseState<EState,ECharacter> CurrentState { get; protected set; }
//         protected EState nextStateKey;

//         protected bool IsTransitioningState = false;

//         protected virtual void Start()
//         {
//             CurrentState?.EnterState();
//         }

//         private void Update()
//         {
//             if(CurrentState == null) return; 
//             if (!IsTransitioningState && (nextStateKey != null && !nextStateKey.Equals(CurrentState.StateKey)))
//             {
//                 TransitionToState(nextStateKey);
//             }

//             CurrentState?.UpdateState();
//             CheckAutoTransition();
//         }
//         private void TransitionToState(EState stateKey)
//         {
//             if (IsTransitioningState) return;
//             if (!states.ContainsKey(stateKey))
//             {
//                 Debug.LogWarning($"[StateManager] 등록되지 않은 상태: {stateKey}");
//                 return;
//             }
//             IsTransitioningState = true;

//             CurrentState?.ExitState();
//             PreviousStateKey = CurrentState != null ? CurrentState.StateKey : stateKey;

//             CurrentState = states[stateKey];
//             CurrentState.EnterState();

//             IsTransitioningState = false;
//         }
//         protected virtual void CheckAutoTransition() { }
//     }
// }