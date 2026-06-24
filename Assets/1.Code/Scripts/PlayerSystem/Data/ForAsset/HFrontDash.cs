using UnityEngine;

namespace Refactoring
{
    [CreateAssetMenu(fileName = "HFrontDash", menuName = "Data/HFrontDash")]
    public class HFrontDash : BaseStateData
    {
        public override PlayerStateType StateType => PlayerStateType.Dash;

    }
}