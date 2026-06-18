using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HNormalAttack3Data", menuName = "Data/HNormalAttack3Data")]
    public class HNormalAttack3Data : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.NormalAttack3;

    }
}