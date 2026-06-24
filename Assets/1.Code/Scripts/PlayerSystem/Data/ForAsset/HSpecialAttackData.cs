using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HSpecialAttackData", menuName = "Data/HSpecialAttackData")]
    public class HSpecialAttackData : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.SpecialAttack;

    }
}