using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 역할: 처형 가능한 대상이 처음 나타나는 순간 EventButtonGuide로 피니쉬 버튼을 강조한다.
    // 튜토리얼성 1회 안내라, 한 번 띄운 뒤로는 다시 검사하지 않는다.
    public class EventFinishGuide : MonoBehaviour
    {
        [SerializeField] private EventButtonGuide guide; // 실제 강조는 여기에 그대로 맡김

        [Preserve, Inject(true)] private IFinishChecker _finishChecker;

        private void Update()
        {
            if (_finishChecker == null || guide == null) return;

            if (_finishChecker.CanFinish())
            {
                guide.Execute();
                enabled = false; // 한 번 띄웠으면 끝. 더 이상 매프레임 검사 안 함
            }
        }
    }
}
