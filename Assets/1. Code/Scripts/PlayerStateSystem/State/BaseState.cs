using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public struct TimingEntry
    {
        public TimingEntry(float progress, StateDataCategoryType categoryType, IHasTimingData data)
        {
            Progress = progress;
            CategoryType = categoryType;
            Data = data;
        }

        [Range(0,1)] public float Progress;
        public StateDataCategoryType CategoryType;
        public IHasTimingData Data;
    }
    
    //EState로 제네릭을 만든 이유는 몬스터에서도 상태머신  방식을 사용하기 위함.
    public abstract class BaseState<EState> where EState : Enum
    {
        public EState StateKey { get; protected set; }
        public bool CanReenter { get; protected set; } = false; // false이면 동일 상태 재진입 요청 무시
        protected static IStateContext Manager { get; private set;}
        protected static IStateDataProvider DataProvider { get; private set;}
        protected static PlayerStateEventChannel EventChannel { get; private set;}
        protected Dictionary<StateDataCategoryType, List<IHasTimingData>> timingDict;
        protected int currentIndex = 0;
        protected ITimingData animationTimingData;
        private TimingEntry[] sortedList;
        protected Animator animator;
        protected int animationHash;
        protected string animationName;

        public static void BindContext(IStateContext manager)
        {
            if(Manager != null) return;

            Manager = manager;
            EventChannel = manager.EventChannel;
            DataProvider = manager.StateDataManager;
        }

        public virtual void EnterState()
        {
#if UNITY_EDITOR
            SortByProgress();
#endif
            animator.CrossFade(animationName, 0.1f, 0,0f);
        }
        
        public virtual void UpdateState()
        {
            float progress;

            if (animator.IsInTransition(0)) //레이어 0이 전환 중(블렌딩)인지 확인
            {
                var nextInfo = animator.GetNextAnimatorStateInfo(0);
                if (nextInfo.shortNameHash != animationHash) return;
                progress = Mathf.Clamp01(nextInfo.normalizedTime);
            }
            else
            {
                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash != animationHash) return;
                progress = Mathf.Clamp01(info.normalizedTime);
            }
            
            //if문으로 하니까 프레임 내에 다 수행을 못함. 1000개 데이터라고 했을 때, 매 프레임마다 실행
            // if(currentIndex >= sortedList.Length) return;
            // if(progress >= sortedList[currentIndex].Progress)
            // {
            //     EventChannel.Raise(sortedList[currentIndex].CategoryType, sortedList[currentIndex].Data);
            //     currentIndex++;
            // }

            while(currentIndex < sortedList.Length
                    && (progress >= sortedList[currentIndex].Progress))
            {
                EventChannel.Raise(sortedList[currentIndex].CategoryType, sortedList[currentIndex].Data);
                currentIndex++;
            }
        }

        public virtual void ExitState()
        {
            currentIndex = 0;
            EventChannel.RaiseReset();
        }

        //모든 startProgress를 한 곳에 모아 진행률 오름차순으로 정렬
        protected void SortByProgress()
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