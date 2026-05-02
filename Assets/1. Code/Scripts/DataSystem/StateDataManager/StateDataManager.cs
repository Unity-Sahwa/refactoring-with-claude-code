using System;
using UnityEngine;
using System.Collections.Generic;

namespace Refactoring
{
    public class StateDataManager : MonoBehaviour, IInjectTarget, IStateDataProvider
    {
        
        [SerializeField] private List<StateDataEntry> _stateDataList; //Inspector에 노출되는 용도
        private readonly Dictionary<PlayerStateType, ScriptableObject> _stateDataMap = new(); //실제 사용되는 변수

        public Type[] InterfaceTypes => new Type[] { typeof(IStateDataProvider) };

        private void Awake()
        {
            foreach (var entry in _stateDataList)
            {
                _stateDataMap[entry.stateType] = entry.data;
            }
        }

        public T GetData<T>(PlayerStateType stateType) where T : class
        {
            //stateType이 없으면 if문 탈출, 있으면 data 반환
            if (_stateDataMap.TryGetValue(stateType, out var data))
            {
                return data as T;
            }

            Debug.LogError($"StateData of type {stateType} not found.");
            return null;
        }
    }
}
