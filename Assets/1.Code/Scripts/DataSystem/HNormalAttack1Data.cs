using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HNormalAttack1Data", menuName = "Data/HNormalAttack1Data")]
    public class HNormalAttack1Data : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.HNormalAttack1;
    }
}