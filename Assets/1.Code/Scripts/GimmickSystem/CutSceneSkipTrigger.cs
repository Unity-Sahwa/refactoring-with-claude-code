using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Refactoring
{
    // 역할: 컷씬의 건너뛰기 가능 구간을 켜고 끄고, 스킵 입력을 받으면 이벤트들을 실행한다.
    // 구간 지정은 타임라인 Signal이 SetSkippable(true/false)를 직접 호출해서 한다.
    // Signal을 안 꽂은 컷씬은 스킵 불가가 되고, 그 동안 ESC는 평소대로 메뉴를 연다.
    public class CutSceneSkipTrigger : MonoBehaviour
    {
        [Tooltip("모바일용 스킵 버튼. 건너뛰기 가능 구간에서만 켜진다.")]
        [SerializeField] private Canvas _skipCanvas;
        [SerializeField] private Button _skipButton;

        [Tooltip("건너뛰었을 때 실행할 이벤트들. (예: 페이드, 타임라인 종료, 컷씬 락 해제)")]
        [SerializeField]
        private List<EventData> _skipEvents;

        [Tooltip("각 이벤트를 몇 초 뒤에 실행할지. 비워둔 칸은 0초.")]
        [SerializeField]
        private List<float> _delayTimes;

        [Preserve, Inject(true)] private ICutsceneInputProvider _cutsceneInput;

        // 컷씬은 동시에 하나만 재생되므로 지금 스킵을 받을 대상도 항상 하나다.
        public static CutSceneSkipTrigger Active { get; private set; }

        private void Awake()
        {
            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(Skip);
            }
            if (_cutsceneInput != null)
            {
                _cutsceneInput.OnCutscenePressed += HandleCutscenePressed;
            }
            SetSkippable(false);
        }

        private void OnDestroy()
        {
            if (_cutsceneInput != null)
            {
                _cutsceneInput.OnCutscenePressed -= HandleCutscenePressed;
            }
        }

        // 컷씬 오브젝트가 꺼질 때 구간이 켜진 채로 남는 걸 막는다.
        private void OnDisable()
        {
            SetSkippable(false);
        }

        // 씬에 트리거가 여럿 있어도 지금 건너뛰기 구간인 하나만 반응한다.
        private void HandleCutscenePressed(InputActionType action)
        {
            if (action == InputActionType.Interaction && Active == this)
            {
                Skip();
            }
        }

        /// <summary>
        /// 타임라인 Signal에서 호출한다. 켜면 건너뛰기 구간 시작, 끄면 끝.
        /// </summary>
        public void SetSkippable(bool canSkip)
        {
            if (canSkip)
            {
                Active = this;
            }
            else if (Active == this)
            {
                Active = null;
            }

            if (_skipCanvas != null)
            {
                _skipCanvas.gameObject.SetActive(canSkip);
            }
        }

        /// <summary>
        /// 컷씬을 건너뛴다. ESC와 스킵 버튼이 같이 쓴다.
        /// </summary>
        public void Skip()
        {
            SetSkippable(false);

            for (int i = 0; i < _skipEvents.Count; i++)
            {
                if (_skipEvents[i] == null)
                {
                    continue;
                }

                float delay = i < _delayTimes.Count ? _delayTimes[i] : 0f;
                StartCoroutine(CoExecuteEvent(_skipEvents[i], delay));
            }
        }

        private IEnumerator CoExecuteEvent(EventData skipEvent, float delay)
        {
            yield return new WaitForSeconds(delay);
            skipEvent.Execute();
        }
    }
}
