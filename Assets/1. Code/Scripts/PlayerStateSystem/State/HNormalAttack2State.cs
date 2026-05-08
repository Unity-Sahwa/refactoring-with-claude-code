using UnityEngine;

namespace Refactoring
{
    public class HNormalAttack2State : BaseState<PlayerStateType>
    {
        public HNormalAttack2State( )
        {
            animationName = "HNormalAttack2";
            StateKey = PlayerStateType.HNormalAttack2;

            animator = Manager.HAnimator;
            animationHash = Animator.StringToHash(animationName);
            CanReenter = false;
            animationTimingData = Manager.StateDataManager.GetData<ITimingData>(StateKey);

            SortByProgress();
        }
    }
}
