using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "LocomotionData", menuName = "Data/LocomotionData")]
    public class LocomotionData : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.Locomotion;
    }
}