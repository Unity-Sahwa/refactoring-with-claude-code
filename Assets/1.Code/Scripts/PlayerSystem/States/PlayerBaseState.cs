using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    //애니메이션을 보유하는 캐릭터의 상태를 추적하는 부모클래스
    public abstract class CharacterBaseState<EState,ECharacter> where EState : Enum where ECharacter : Enum
    {
        
        public abstract bool CanReenter {get;}
        public abstract EState StateKey {get;}
        public abstract ECharacter CharacterType {get;}
        protected abstract string AnimationName {get;}
        private int _currentIndex = 0;
        private int _animationHash;
        protected Animator CharaterAnim {get; private set;}
        public IStateEventRaiser EventRaiser {get; private set;}
        public ITimingData<EState> TimingData {get; private set;}
        private List<(float, StateEventCategory, int)> sortedList = new List<(float, StateEventCategory, int)>();

        public CharacterBaseState()
        {
            _animationHash = Animator.StringToHash(AnimationName);
            SortTimingData();
        }

        public void Initialize(Animator animator, IStateEventRaiser eventRaiser, ITimingData<EState> timingData)
        {
            CharaterAnim = animator;
            EventRaiser = eventRaiser;
            TimingData = timingData;
        }        
        

        public virtual void EnterState()
        {
#if UNITY_EDITOR
            SortTimingData();
#endif
            CharaterAnim.CrossFade(AnimationName, 0.1f, 0,0f);
        }
        public virtual void UpdateState()
        {
            float progress;

            if (CharaterAnim.IsInTransition(0)) //레이어 0이 전환 중(블렌딩)인지 확인
            {
                var nextInfo = CharaterAnim.GetNextAnimatorStateInfo(0);
                if (nextInfo.shortNameHash != _animationHash) return;
                progress = Mathf.Clamp01(nextInfo.normalizedTime);
            }
            else
            {
                var info = CharaterAnim.GetCurrentAnimatorStateInfo(0);
                if (info.shortNameHash != _animationHash) return;
                progress = Mathf.Clamp01(info.normalizedTime);
            }
            
            //STUDY: if문으로 진행할 경우 프레임마다 실행하기 때문에 의도된 결과가 나오지 않음.
            while(_currentIndex < sortedList.Count
                    && progress >= sortedList[_currentIndex].Item1)
            {
                var entry = sortedList[_currentIndex];
                EventRaiser.Raise(entry.Item2, entry.Item3);
                _currentIndex++;
            }
        }
        public virtual void ExitState()
        {
            _currentIndex = 0;
            EventRaiser.RaiseReset();
        }

        //모든 startProgress를 한 곳에 모아 진행률 오름차순으로 정렬
        private void SortTimingData()
        {
            Dictionary<StateEventCategory, List<IHasTimingData>> progressMap = TimingData.GetAllTimingData(); 
            sortedList.Clear();
            
            foreach (var (category, dataList) in progressMap)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    sortedList.Add((dataList[i].StartProgress,category, i));
                }
            }

            #region STUDY
            //STUDY: Sort(Comparison<T> comparison) 분석(feat: Delegate, Lamda)
            // 내부정의1. public void Sort(Comparison<T> comparison);
            // 내부정의2. public delegate int Comparison<in T>(T x, T y);
            // -> Sort의 매개변수는 (2개의 T를 매개변수로 두고, int를 반환하는 delegate)
            
            // sortedList.Sort((a,b) => a.Item1.CompareTo(b.Item1));
            // 분석1: 제네릭 <T>는 a,b의 타입. a,b는 sortedList 내에 있는 튜플(float, AnimationEventCategory, int)임.
            // 반환값은 int인 a.Item1.CompareTo(b.Item1)
            //  반환값이 음수 -> a가 b보다 앞에 위치
            //  반환값이 0 -> 순서유지
            //  반환값이 양수 -> b가 a보다 앞에 위치
            #endregion            
            sortedList.Sort((a,b) => a.Item1.CompareTo(b.Item1));
        }
    }
}