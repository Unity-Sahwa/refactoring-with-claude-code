using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Refactoring
{
    public class HNormalAttack1State : BaseState<PlayerStateType>
    {
        private ITimingData animationTimingData;
        private Animator hAnimator;
        private static readonly int HNormalAttack1Hash = Animator.StringToHash("HNormalAttack1");
        private Dictionary<StateDataCategoryType, List<IHasTimingData>> timingDict;
        private TimingEntry[] sortedList;
        private int currentIndex;
        private PlayerStateEventChannel eventChannel;

        public HNormalAttack1State(IStateContext manager) : base(manager)
        {
            StateKey = PlayerStateType.HNormalAttack1;
            hAnimator = manager.HAnimator;
            eventChannel = manager.EventChannel;
            animationTimingData = manager.StateDataManager.GetData<ITimingData>(PlayerStateType.HNormalAttack1);

            SortByProgress();
            currentIndex = 0;
        }

        public override void EnterState()
        {
#if UNITY_EDITOR
            SortByProgress();
#endif
            currentIndex = 0;
            hAnimator.CrossFade("HNormalAttack1", 0.5f, 0,0f);

        }

        public override void UpdateState()
        {
            float progress;

            if (hAnimator.IsInTransition(0)) //레이어 0이 전환 중(블렌딩)인지 확인
            {
                var nextInfo = hAnimator.GetNextAnimatorStateInfo(0);
                if (nextInfo.shortNameHash != HNormalAttack1Hash) return;
                progress = Mathf.Clamp01(nextInfo.normalizedTime);
            }
            else
            {
                var info = hAnimator.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash != HNormalAttack1Hash) return;
                progress = Mathf.Clamp01(info.normalizedTime);
            }

            if(currentIndex >= sortedList.Length) return;
            if(progress >= sortedList[currentIndex].Progress)
            {
                Debug.Log($"animation Progress: {progress}");
                Debug.Log($"event Progress: {sortedList[currentIndex].Progress}");
                eventChannel.Raise(sortedList[currentIndex].CategoryType, sortedList[currentIndex].Data);
                currentIndex++;
            }
        }

        public override void ExitState()
        {
            eventChannel.RaiseReset();
        }

        //모든 startProgress를 한 곳에 모아 진행률 오름차순으로 정렬
        private void SortByProgress()
        {
            timingDict = animationTimingData.GetAllTimingData();

            var entries = new List<TimingEntry>();
            foreach (var pair in timingDict)
            {
                foreach (var data in pair.Value)
                {
                    entries.Add(new TimingEntry(data.StartProgress, pair.Key, data));
                }
            }

            entries.Sort((a,b) => a.Progress.CompareTo(b.Progress));
            sortedList = entries.ToArray();
        }
    }
}
