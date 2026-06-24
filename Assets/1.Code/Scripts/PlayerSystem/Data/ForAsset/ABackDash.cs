using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "ABackDash", menuName = "Data/ABackDash")]
    public class ABackDash : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.LockOnDash;

    }
}