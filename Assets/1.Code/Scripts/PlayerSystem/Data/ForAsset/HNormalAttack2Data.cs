using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HNormalAttack2Data", menuName = "Data/HNormalAttack2Data")]
    public class HNormalAttack2Data : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.NormalAttack2;
    }
}