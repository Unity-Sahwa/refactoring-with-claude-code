using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HNormalAttack1Data", menuName = "Data/HNormalAttack1Data")]
    public class HNormalAttack1Data : ScriptableObject, ITimingData<PlayerStateType>
    {
        [SerializeField] private InputDataEntry[] input;
        [SerializeField] private SkillEffectDataEntry[] effect;
        [SerializeField] private AudioDataEntry[] audio;

        public PlayerStateType StateType => PlayerStateType.HNormalAttack1;

        //외부에 타이밍 데이터를 
        public Dictionary<StateEventCategory, List<IHasTimingData>> GetAllTimingData()
        {
            var dict = new Dictionary<StateEventCategory, List<IHasTimingData>>();
            
            dict[StateEventCategory.Input] = input.Cast<IHasTimingData>().ToList();
            dict[StateEventCategory.Effect] = effect.Cast<IHasTimingData>().ToList();
            dict[StateEventCategory.Audio] = audio.Cast<IHasTimingData>().ToList();

            return dict;
        }
    }
}