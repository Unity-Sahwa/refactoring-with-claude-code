using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public abstract class BaseStateData : ScriptableObject
    {
        [SerializeField] private InputDataEntry[] input;
        [SerializeField] private SkillMoveDataEntry[] skillMove;
        [SerializeField] private SkillEffectDataEntry[] effect;
        [SerializeField] private HitboxDataEntry[] hitbox;
        [SerializeField] private AudioDataEntry[] audio;

        public abstract PlayerStateType StateType {get;}

        private Dictionary<StateEventCategory, Array> _dataMap;

        private void OnEnable() => BuildDataMap();
        private void BuildDataMap()
        {
            _dataMap = new()
            {
                [StateEventCategory.Input] = input,
                [StateEventCategory.SkillMove] = skillMove,
                [StateEventCategory.Effect] = effect,
                [StateEventCategory.Hitbox] = hitbox,
                [StateEventCategory.Audio] = audio,
            };
        }

        public Dictionary<StateEventCategory,T[]> GetData<T>()
        {
            // OnEnable() 문제가 생길경우 _dataMap이 누락됨.
            if(_dataMap == null) BuildDataMap();

            var result = new Dictionary<StateEventCategory, T[]>();
            
            foreach (var (category, array) in _dataMap)
            {
                if (array == null || array.Length == 0) continue;
                
                if(array is T[] typed)
                {
                    result[category] = typed;
                }
            }
            return result;
        }
    }
}