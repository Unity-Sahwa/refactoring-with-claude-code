using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactoring
{
    public class PlayerStateManager : StateManager<PlayerStateType,PlayerCharacterType>, 
                    IInterfaceInjectable, IAbstractTypeInjected, IDataInjectable
    {
        private static PlayerStateManager _instance;
        public Dictionary<Type, List<object>> injectedImplements { get; } = new Dictionary<Type, List<object>>()
        {
            { typeof(BaseCharacter<PlayerCharacterType>), new List<object>() },
            { typeof(IInputEventProvider), new List<object>() },
            { typeof(ICharacterSwapNotifier), new List<object>() }
        };
        public Dictionary<Type, List<Type>> AbstractTypeMap {get;} = new Dictionary<Type, List<Type>>() 
        {
            { typeof(CharacterBaseState<PlayerStateType,PlayerCharacterType>), new List<Type>() }
        };
        public Dictionary<Type, List<ScriptableObject>> RequiredData {get;} = new Dictionary<Type, List<ScriptableObject>> ()
        {
            //이벤트채널
            {typeof(BaseStateData), new List<ScriptableObject>()},
            {typeof(IPlayerStateEventRaiser), new List<ScriptableObject>()} 
        };
        private Dictionary<PlayerCharacterType, Animator> _playerAnimMap = new Dictionary<PlayerCharacterType, Animator>();
        private Dictionary<PlayerStateType, BaseStateData> _stateDataMap = new Dictionary<PlayerStateType, BaseStateData>();
        private PlayerStateEventChannel _eventRaiser;
        private IInputEventProvider _inputBroadcaster;
        private ICharacterSwapNotifier _characterSwapNotifier;
        private readonly Dictionary<PlayerCharacterType, PlayerStateType> _characterSwapMap = new()
        {
            {PlayerCharacterType.HumanCharacter, PlayerStateType.HIdle},
            {PlayerCharacterType.AnimalCharacter, PlayerStateType.AIdle}
        };

        private void Awake()
        {
            if(_instance == null)
            {
                _instance = this;
            }
            else if(_instance != this)
            {
                Destroy(gameObject);
            }


            var characterList = injectedImplements[typeof(BaseCharacter<PlayerCharacterType>)];
            foreach (var character in characterList)
            {
                var playerCharacter = (BaseCharacter<PlayerCharacterType>)character;
                _playerAnimMap[playerCharacter.Type] = playerCharacter.GetCharacterComponent<Animator>();
            }
            
            var dataList = RequiredData[typeof(BaseStateData)];
            foreach (var data in dataList)
            {
                var stateData = (BaseStateData)data;
                _stateDataMap[stateData.StateType] = stateData;
            }

            _eventRaiser = (PlayerStateEventChannel)RequiredData[typeof(IPlayerStateEventRaiser)][0];
            _inputBroadcaster = (IInputEventProvider)injectedImplements[typeof(IInputEventProvider)][0];
            
            _characterSwapNotifier = (ICharacterSwapNotifier)injectedImplements[typeof(ICharacterSwapNotifier)][0];
            _characterSwapNotifier.OnCharacterSwapped += OnCharacterSwapped;

            CreateStates();

            CurrentState = states.First().Value;
            nextStateKey = CurrentState.StateKey;
        }

        protected override void Start()
        {
            _inputBroadcaster.OnInputPressed += Test_OnInputPressed; //테스트

            base.Start();
        }
        private void CreateStates()
        {
            foreach (var classType in AbstractTypeMap[typeof(CharacterBaseState<PlayerStateType,PlayerCharacterType>)])
            {   
                //STUDY Activator.CreateInstance로 타입으로 인스턴스 생성.
                var instance = (CharacterBaseState<PlayerStateType,PlayerCharacterType>)Activator.CreateInstance(classType);

                if(!_playerAnimMap.ContainsKey(instance.CharacterType)) continue;
                if(!_stateDataMap.ContainsKey(instance.StateKey)) continue;

                instance.Initialize(_playerAnimMap[instance.CharacterType], _eventRaiser, _stateDataMap[instance.StateKey]);
                states[instance.StateKey] = instance;
            }
        }
        private void OnCharacterSwapped(PlayerCharacterType type)
        {
            var characterIdle = _characterSwapMap[type];
            if (!states.ContainsKey(characterIdle))
            {
                Debug.LogError($"{type}의 {characterIdle}이 생성되지 않았습니다.");
                return;
            }

            nextStateKey = characterIdle;
        }
        protected override void CheckAutoTransition()
        {
            if(CurrentState.IsFinished
            && _characterSwapMap.TryGetValue(CurrentState.CharacterType, out var idle)
            && !CurrentState.StateKey.Equals(idle))
            {
                nextStateKey = idle;
            }
        }
        private void Test_OnInputPressed(InputActionType actionType)
        {
            if(actionType == InputActionType.NormalAttack)
            {
                if(states.TryGetValue(PlayerStateType.HNormalAttack1, out var attack1) && CurrentState == attack1
                   && states.TryGetValue(PlayerStateType.HNormalAttack2, out var next1))
                {
                    nextStateKey = PlayerStateType.HNormalAttack2;
                }
                else if(states.TryGetValue(PlayerStateType.HNormalAttack2, out var attack2) && CurrentState == attack2
                   && states.TryGetValue(PlayerStateType.HNormalAttack3, out var next2))
                {
                    nextStateKey = PlayerStateType.HNormalAttack3;
                }
                else
                {
                    nextStateKey = PlayerStateType.HNormalAttack1;
                }
            }
        }
    
        void OnDestroy()
        {
            if(_characterSwapNotifier != null)
            {
                _characterSwapNotifier.OnCharacterSwapped -= OnCharacterSwapped;
            }
        }
    }
}