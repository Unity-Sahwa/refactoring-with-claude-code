using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "StateData", menuName = "Data/StateData")]
    public class StateData : ScriptableObject
    {
        [SerializeField] private PlayerStateType stateType;   // 상태 정체성. 자식 클래스 override 대신 인스펙터에서 지정
        [SerializeField] private bool isLooping;

        [Space(10f)]
        [SerializeField] private InputControlDataEntry[] inputBlock;
        [SerializeField] private InputControlDataEntry[] inputBuffer;
        [SerializeField] private SkillMoveDataEntry[] skillMove;
        [SerializeField] private MotionControlDataEntry[] moveControl;
        [SerializeField] private MotionControlDataEntry[] rotateControl;
        [SerializeField] private SkillEffectDataEntry[] effect;
        [SerializeField] private HitboxDataEntry[] hitbox;
        [SerializeField] private AudioDataEntry[] audio;
        [SerializeField] private TimingDataEntry[] superArmor;
        [SerializeField] private TimingDataEntry[] invincible;
        
        public PlayerStateType StateType => stateType;
        public bool IsLooping => isLooping;
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
            // OnEnable() 문제가 생길경우 _dataMap이 누락될 경우 방지
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