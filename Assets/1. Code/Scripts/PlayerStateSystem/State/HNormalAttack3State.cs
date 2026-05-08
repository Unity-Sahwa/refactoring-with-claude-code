using UnityEngine;

namespace Refactoring
{
    public class HNormalAttack3State : BaseState<PlayerStateType>
    {
        public HNormalAttack3State( )
        {
            animationName = "HNormalAttack3";
            StateKey = PlayerStateType.HNormalAttack3;

            animator = Manager.HAnimator;
            animationHash = Animator.StringToHash(animationName);
            CanReenter = false;
            animationTimingData = Manager.StateDataManager.GetData<ITimingData>(StateKey);

            SortByProgress();
        }
    }
}
