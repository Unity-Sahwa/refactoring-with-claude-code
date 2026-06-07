using System;
using System.Collections.Generic;
using System.Linq;
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
        
        private int _readIndex = 0;
        private int _animationHash;
        protected Animator CharaterAnim {get; private set;}
        public IPlayerStateEventRaiser EventRaiser {get; private set;}
        public BaseStateData StateData {get; private set;}
        private readonly List<(StateEventCategory Category, IStartData Data)> _sortedEvents = new();
        public void Initialize(Animator animator, IPlayerStateEventRaiser eventRaiser, BaseStateData stateData)
        {
            CharaterAnim = animator;
            EventRaiser = eventRaiser;
            StateData = stateData;

            _animationHash = Animator.StringToHash(AnimationName);
            SortEvents();
        }        
        
        public virtual void EnterState()
        {
#if UNITY_EDITOR
            SortEvents();
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
            while (_readIndex < _sortedEvents.Count
                    && progress >= _sortedEvents[_readIndex].Data.StartProgress)
            {
                var (category, data) = _sortedEvents[_readIndex];
                EventRaiser.Raise(category, data);
                _readIndex++;
            }
        }
        public virtual void ExitState()
        {
            _readIndex = 0;
            EventRaiser.RaiseReset();
        }

        //모든 startProgress를 한 곳에 모아 진행률 오름차순으로 정렬
        private void SortEvents()
        {
            Dictionary<StateEventCategory,IStartData[]> progressMap = StateData.GetData<IStartData>();

            _sortedEvents.Clear();
            
            foreach (var (category, dataArray) in progressMap)
            {
                foreach (IStartData data in dataArray)
                {
                    _sortedEvents.Add((category, data));
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
            _sortedEvents.Sort((a, b) => a.Data.StartProgress.CompareTo(b.Data.StartProgress));
        }
    }
}