using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "ASpecialAttackData", menuName = "Data/ASpecialAttackData")]
    public class ASpecialAttackData : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.SpecialAttack;

    }
}