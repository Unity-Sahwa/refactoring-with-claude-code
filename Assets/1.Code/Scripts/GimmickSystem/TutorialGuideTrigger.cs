using UnityEngine;

namespace Refactoring
{
    public class tutorialGuideTrigger : EventData
    {
        //[SerializeField] private TutorialState state;
        public override void Execute()
        {
            //PlayGuide.instance.ShowTutorialUI(state);
        }
    }
}

