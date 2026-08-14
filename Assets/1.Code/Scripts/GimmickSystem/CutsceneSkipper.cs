using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Refactoring
{
    // 역할: 컷씬의 건너뛰기 가능 구간을 켜고 끄고, 스킵 입력을 받아 타임라인을 끝으로 보낸다.
    // 구간 지정은 타임라인 Signal이 SetSkippable(true/false)를 직접 호출해서 한다.
    // Signal을 안 꽂은 컷씬은 스킵 불가가 되고, 그 동안 ESC는 평소대로 메뉴를 연다.
    [RequireComponent(typeof(PlayableDirector))]
    public class CutsceneSkipper : MonoBehaviour
    {
        [Tooltip("모바일용 스킵 버튼. 건너뛰기 가능 구간에서만 켜진다.")]
        [SerializeField]
        private Button _skipButton;

        private PlayableDirector _director;

        // 컷씬은 동시에 하나만 재생되므로 지금 스킵을 받을 대상도 항상 하나다.
        public static CutsceneSkipper Active { get; private set; }

        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(Skip);
            }
            SetSkippable(false);
        }

        // 컷씬 오브젝트가 꺼질 때 구간이 켜진 채로 남는 걸 막는다.
        private void OnDisable()
        {
            SetSkippable(false);
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

            if (_skipButton != null)
            {
                _skipButton.gameObject.SetActive(canSkip);
            }
        }

        /// <summary>
        /// 컷씬을 건너뛴다. ESC와 스킵 버튼이 같이 쓴다.
        /// </summary>
        public void Skip()
        {
            SetSkippable(false);

            // Stop만 하면 연출이 중간 상태로 멈추므로, 끝 시점을 한 번 적용하고 멈춘다.
            _director.time = _director.duration;
            _director.Evaluate();
            _director.Stop();
        }
    }
}
