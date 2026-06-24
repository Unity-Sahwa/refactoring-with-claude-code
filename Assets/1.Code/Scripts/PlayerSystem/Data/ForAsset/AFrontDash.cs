using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "AFrontDash", menuName = "Data/AFrontDash")]
    public class AFrontDash : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.Dash;

    }
}