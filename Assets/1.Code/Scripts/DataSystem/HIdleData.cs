using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HIdleData", menuName = "Data/HIdleData")]
    public class HIdleData : ScriptableObject, ITimingData<PlayerStateType>
    {
        [SerializeField] private InputDataEntry[] input;
        [SerializeField] private SkillEffectDataEntry[] effect;
        [SerializeField] private AudioDataEntry[] audio;

        public PlayerStateType StateType {get;} = PlayerStateType.HIdle;

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
