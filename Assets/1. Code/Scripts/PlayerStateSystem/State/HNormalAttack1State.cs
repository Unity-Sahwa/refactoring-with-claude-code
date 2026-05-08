using UnityEngine;

namespace Refactoring
{
    public class HNormalAttack1State : BaseState<PlayerStateType>
    {
        public HNormalAttack1State( )
        {
            animationName = "HNormalAttack1";
            StateKey = PlayerStateType.HNormalAttack1;

            animator = Manager.HAnimator;
            animationHash = Animator.StringToHash(animationName);
            CanReenter = false;
            animationTimingData = Manager.StateDataManager.GetData<ITimingData>(StateKey);

            SortByProgress();
        }
    }
}
