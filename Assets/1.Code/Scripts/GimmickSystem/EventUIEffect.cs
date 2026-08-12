using UnityEngine;

namespace Refactoring
{
    // 역할: 기믹이 발동하면 지정한 UIEffectTarget의 효과를 재생함.
    public class EventUIEffect : EventData
    {
        [Inject] private UIEffectRunner _runner;
        [SerializeField] private UIEffectTarget target;

        public override void Execute()
        {
            if (_runner == null)
            {
                Debug.LogError("EventUIEffect에 UIEffectRunner 주입 실패");
                return;
            }

            _runner.Play(target);
        }
    }
}
