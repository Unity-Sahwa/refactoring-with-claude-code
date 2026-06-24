using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "ANormalAttack2Data", menuName = "Data/ANormalAttack2Data")]
    public class ANormalAttack2Data : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.NormalAttack2;
    }
}