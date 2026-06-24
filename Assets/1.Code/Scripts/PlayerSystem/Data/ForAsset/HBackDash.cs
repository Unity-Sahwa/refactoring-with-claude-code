using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HBackDash", menuName = "Data/HBackDash")]
    public class HBackDash : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.LockOnDash;

    }
}