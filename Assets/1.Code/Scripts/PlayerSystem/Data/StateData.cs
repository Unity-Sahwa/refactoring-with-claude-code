using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    //역할: 플레이어 상태 이벤트의 구독자에게 전달될 데이터. 필요한 데이터를 찾아서 얻을 수 있다.
    //데이터를 왜 한 곳에 모았는가?: 테스트시, 상태마다 연출 조정하기 쉬우려고
    //많으면 복잡하지 않는가?: 없는 변수들을 Inspector에서 가려지도록 Editor에서 처리함. 복잡해 보이는 것은 실제로 변수들이 많기 때문이고, Editor로 원하는 것만 보이도록 구현가능

    [CreateAssetMenu(fileName = "StateData", menuName = "Data/StateData")]
    public class StateData : ScriptableObject
    {
        [SerializeField] private PlayerStateType stateType;   // 상태 정체성. 자식 클래스 override 대신 인스펙터에서 지정
        [SerializeField] private bool isLooping;

        [Space(10f)]
        [SerializeField] private IntervalDataEntry[] inputBlock;
        [SerializeField] private IntervalDataEntry[] inputBuffer;
        [SerializeField] private IntervalDataEntry[] moveControl;
        [SerializeField] private IntervalDataEntry[] rotateControl;
        [SerializeField] private IntervalDataEntry[] superArmor;
        [SerializeField] private IntervalDataEntry[] invincible;
        [SerializeField] private SkillMoveDataEntry[] skillMove;
        
        [SerializeField] private SkillEffectDataEntry[] effect;
        [SerializeField] private HitboxDataEntry[] hitbox;
        [SerializeField] private AudioDataEntry[] audio;
        
        
        public PlayerStateType StateType => stateType;
        public bool IsLooping => isLooping;
        private Dictionary<StateEventCategory, Array> _dataMap;

        private void OnEnable() => BuildDataMap();

#if UNITY_EDITOR
        private void OnValidate() => BuildDataMap();
#endif
        
        //왜 이렇게 구현? : 2군데서 호출하기에 메서드로 만듬. 
        //                  (OnEnable 이전에 GetData가 호출되면 맵이 형성이 안되어 있기 때문에 GetData에서도 BuildDataMap 호출하도록 구성)
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
        
        // 왜구현? : StateData에서 구현된 데이터 중에 원하는 데이터만 가져다가 쓰기 위함.
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