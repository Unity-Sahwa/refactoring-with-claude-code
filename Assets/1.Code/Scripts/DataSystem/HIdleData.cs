using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HIdleData", menuName = "Data/HIdleData")]
    public class HIdleData : BaseStateData
    {
        public override PlayerStateType StateType {get;} = PlayerStateType.Locomotion;
    }
}
