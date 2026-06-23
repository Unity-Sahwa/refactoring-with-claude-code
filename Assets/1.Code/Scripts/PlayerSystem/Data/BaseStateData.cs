using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public abstract class BaseStateData : ScriptableObject
    {
        [SerializeField] private InputControlDataEntry[] inputBlock;
        [SerializeField] private InputControlDataEntry[] inputBuffer;
        [SerializeField] private SkillMoveDataEntry[] skillMove;
        [SerializeField] private MotionControlDataEntry[] moveControl;
        [SerializeField] private MotionControlDataEntry[] rotateControl;
        [SerializeField] private SkillEffectDataEntry[] effect;
        [SerializeField] private HitboxDataEntry[] hitbox;
        [SerializeField] private AudioDataEntry[] audio;
        [SerializeField] private TimingDataEntry[] superArmor;   // 슈퍼아머(피격 경직 무시) 구간
        [SerializeField] private TimingDataEntry[] invincible;   // 무적(피격 자체 무시) 구간

        public abstract PlayerStateType StateType {get;}

        private Dictionary<StateEventCategory, Array> _dataMap;

        private void OnEnable() => BuildDataMap();
        private void BuildDataMap()
        {
            _dataMap = new()
            {
                [StateEventCategory.InputBlock] = inputBlock,
                [StateEventCategory.InputBuffer] = inputBuffer,
                [StateEventCategory.SkillMove] = skillMove,
                [StateEventCategory.MoveControl] = moveControl,
                [StateEventCategory.RotateControl] = rotateControl,
                [StateEventCategory.Effect] = effect,
                [StateEventCategory.Hitbox] = hitbox,
                [StateEventCategory.Audio] = audio,
                [StateEventCategory.SuperArmor] = superArmor,
                [StateEventCategory.Invincible] = invincible,
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