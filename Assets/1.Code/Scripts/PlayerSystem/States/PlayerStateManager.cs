using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class PlayerStateManager : StateManager<PlayerStateType,PlayerCharacterType>, 
                    IInterfaceInjectable, IAbstractTypeInjected, IDataInjectable
    {
        public Dictionary<Type, List<object>> injectedImplements { get; } = new Dictionary<Type, List<object>>()
        {
            { typeof(BaseCharacter<PlayerCharacterType>), new List<object>() },
            { typeof(IInputEventProvider), new List<object>() }
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

        private void Awake()
        {
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

            CreateStates();
        }

        protected override void Start()
        {
            //테스트
            _inputBroadcaster.OnInputPressed += Test_OnInputPressed;
            if(states[PlayerStateType.HIdle] != null) CurrentState = states[PlayerStateType.HIdle];

            base.Start();
        }
        private void CreateStates()
        {
            foreach (var classType in AbstractTypeMap[typeof(CharacterBaseState<PlayerStateType,PlayerCharacterType>)])
            {   
                //STUDY Activator.CreateInstance로 타입으로 인스턴스 생성.
                var instance = (CharacterBaseState<PlayerStateType,PlayerCharacterType>)Activator.CreateInstance(classType);
                instance.Initialize(_playerAnimMap[instance.CharacterType], _eventRaiser, _stateDataMap[instance.StateKey]);
                states[instance.StateKey] = instance;
            }
        }
        private void Test_OnInputPressed(InputActionType actionType)
        {
            if(actionType == InputActionType.NormalAttack)
            {
                if(CurrentState == states[PlayerStateType.HNormalAttack1])
                {
                    nextStateKey = PlayerStateType.HNormalAttack2;
                }
                else if(CurrentState == states[PlayerStateType.HNormalAttack2])
                {
                    nextStateKey = PlayerStateType.HNormalAttack3;
                }
                else
                {
                    nextStateKey = PlayerStateType.HNormalAttack1;
                }
            }
            else if (actionType == InputActionType.SpecialAttack)
            {
                CurrentState = states[ PlayerStateType.HSpecialAttack];
                nextStateKey = PlayerStateType.HSpecialAttack;
            }
            else if (actionType == InputActionType.FinishAttack)
            {
                CurrentState = states[ PlayerStateType.HFinishAttack];
                nextStateKey = PlayerStateType.HFinishAttack;
            }
        }
    }
}