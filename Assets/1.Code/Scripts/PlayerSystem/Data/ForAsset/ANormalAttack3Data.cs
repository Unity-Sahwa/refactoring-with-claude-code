using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "ANormalAttack3Data", menuName = "Data/ANormalAttack3Data")]
    public class ANormalAttack3Data : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.NormalAttack3;

    }
}