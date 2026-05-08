using UnityEngine;

namespace Refactoring
{
    public class HIdleState : BaseState<PlayerStateType>
    {
        public HIdleState( )
        {
            animationName = "HIdle";
            StateKey = PlayerStateType.HIdle;

            animator = Manager.HAnimator;
            animationHash = Animator.StringToHash(animationName);
            CanReenter = false;
            animationTimingData = Manager.StateDataManager.GetData<ITimingData>(StateKey);

            SortByProgress();
        }
    }
}
