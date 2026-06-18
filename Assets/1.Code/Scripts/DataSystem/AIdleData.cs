using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "AIdleData", menuName = "Data/AIdleData")]
    public class AIdleData : BaseStateData
    {
        public override PlayerStateType StateType {get;} = PlayerStateType.Locomotion;
    }
}
