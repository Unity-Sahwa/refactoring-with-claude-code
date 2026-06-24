using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "ANormalAttack1Data", menuName = "Data/ANormalAttack1Data")]
    public class ANormalAttack1Data : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.NormalAttack1;
    }
}