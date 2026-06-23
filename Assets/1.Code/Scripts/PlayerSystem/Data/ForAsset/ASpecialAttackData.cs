using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "ASpecialAttackData", menuName = "Data/ASpecialAttackData")]
    public class SpecialAttackData : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.SpecialAttack;

    }
}