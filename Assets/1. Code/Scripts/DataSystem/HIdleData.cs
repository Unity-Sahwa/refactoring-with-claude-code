using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HIdleData", menuName = "Data/HIdleData")]
    public class HIdleData : ScriptableObject, ITimingData
    {
        [SerializeField] private InputDataClass[] input;
        [SerializeField] private SkillEffectDataClass[] effect;
        [SerializeField] private AudioDataClass[] audio;

        //외부에 타이밍 데이터를 
        public Dictionary<StateDataCategoryType, List<IHasTimingData>> GetAllTimingData()
        {
            var dict = new Dictionary<StateDataCategoryType, List<IHasTimingData>>();
            
            dict[StateDataCategoryType.Input] = input.Cast<IHasTimingData>().ToList();
            dict[StateDataCategoryType.Effect] = effect.Cast<IHasTimingData>().ToList();
            dict[StateDataCategoryType.Audio] = audio.Cast<IHasTimingData>().ToList();

            return dict;
        }
    }
}
