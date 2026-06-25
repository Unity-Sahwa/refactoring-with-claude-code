// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// namespace Refactoring
// {
//     public class PlayerStateManager : StateManager<PlayerStateType,PlayerCharacterType>
//     {
//         private static PlayerStateManager _instance;
//         [Inject] private List<BaseStateData> _stateDataList;
//         [Inject] private PlayerStateEventChannel _eventRaiser;
//         [Inject] private IInputEventProvider _inputBroadcaster;
//         [Inject] private ICharacterSwapNotifier _characterSwapNotifier;
//         [Inject] private List<BaseCharacter<PlayerCharacterType>> _characters;
//         [InjectSubTypes(typeof(CharacterBaseState<PlayerStateType,PlayerCharacterType>))] private List<Type> _stateTypes;
//         private Dictionary<PlayerStateType, BaseStateData> _stateDataMap = new Dictionary<PlayerStateType, BaseStateData>();
//         private Dictionary<PlayerCharacterType, Animator> _playerAnimMap = new Dictionary<PlayerCharacterType, Animator>();
//         private readonly Dictionary<PlayerCharacterType, PlayerStateType> _characterSwapMap = new()
//         {
//             {PlayerCharacterType.HumanCharacter, PlayerStateType.HIdle},
//             {PlayerCharacterType.AnimalCharacter, PlayerStateType.AIdle}
//         };

//         private void Awake()
//         {
//             if(_instance == null)
//             {
//                 _instance = this;
//             }
//             else if(_instance != this)
//             {
//                 Destroy(gameObject);
//             }

//             foreach (var character in _characters)
//             {
//                 _playerAnimMap[character.Type] = character.GetCharacterComponent<Animator>();
//             }
            
//             foreach (var data in _stateDataList)
//             {
//                 _stateDataMap[data.StateType] = data;
//             }

//             _characterSwapNotifier.OnCharacterSwapped += OnCharacterSwapped;

//             CreateStates();

//             CurrentState = states.First().Value;
//             nextStateKey = CurrentState.StateKey;
//         }

//         protected override void Start()
//         {
//             _inputBroadcaster.OnInputPressed += Test_OnInputPressed; //테스트

//             base.Start();
//         }
//         private void CreateStates()
//         {
//             foreach (var classType in _stateTypes)
//             {   
//                 //STUDY Activator.CreateInstance로 타입으로 인스턴스 생성.
//                 var instance = (CharacterBaseState<PlayerStateType,PlayerCharacterType>)Activator.CreateInstance(classType);

//                 if(!_playerAnimMap.ContainsKey(instance.CharacterType)) continue;
//                 if(!_stateDataMap.ContainsKey(instance.StateKey)) continue;

//                 instance.Initialize(_playerAnimMap[instance.CharacterType], _eventRaiser, _stateDataMap[instance.StateKey]);
//                 states[instance.StateKey] = instance;
//             }
//         }
//         private void OnCharacterSwapped(PlayerCharacterType type)
//         {
//             var characterIdle = _characterSwapMap[type];
//             if (!states.ContainsKey(characterIdle))
//             {
//                 Debug.LogError($"{type}의 {characterIdle}이 생성되지 않았습니다.");
//                 return;
//             }

//             nextStateKey = characterIdle;
//         }
//         protected override void CheckAutoTransition()
//         {
//             if(CurrentState.IsFinished
//             && _characterSwapMap.TryGetValue(CurrentState.CharacterType, out var idle)
//             && !CurrentState.StateKey.Equals(idle))
//             {
//                 nextStateKey = idle;
//             }
//         }
//         private void Test_OnInputPressed(InputActionType actionType)
//         {
//             if(actionType == InputActionType.NormalAttack)
//             {
//                 if(states.TryGetValue(PlayerStateType.HNormalAttack1, out var attack1) && CurrentState == attack1
//                    && states.TryGetValue(PlayerStateType.HNormalAttack2, out var next1))
//                 {
//                     nextStateKey = PlayerStateType.HNormalAttack2;
//                 }
//                 else if(states.TryGetValue(PlayerStateType.HNormalAttack2, out var attack2) && CurrentState == attack2
//                    && states.TryGetValue(PlayerStateType.HNormalAttack3, out var next2))
//                 {
//                     nextStateKey = PlayerStateType.HNormalAttack3;
//                 }
//                 else
//                 {
//                     nextStateKey = PlayerStateType.HNormalAttack1;
//                 }
//             }
//         }
    
//         void OnDestroy()
//         {
//             if(_characterSwapNotifier != null)
//             {
//                 _characterSwapNotifier.OnCharacterSwapped -= OnCharacterSwapped;
//             }
//         }
//     }
// }