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
        
        private int _readIndex = 0;
        private int _animationHash;
        private bool _hasStarted;
        public bool IsFinished {get; private set;}
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
# if UNITY_EDITOR
            SortEvents();
# endif
            _hasStarted = false;
            IsFinished = false;
            CharaterAnim.CrossFade(AnimationName, 0.1f, 0,0f);
        }
        public virtual void UpdateState()
        {
            float progress;

            if (CharaterAnim.IsInTransition(0))
            {
                var nextInfo = CharaterAnim.GetNextAnimatorStateInfo(0);
                if (nextInfo.shortNameHash != _animationHash) //현재 애니에서 다음 애니로 전환됨.
                {
                    if(_hasStarted) IsFinished = true;
                    return;
                }

                //이전 애니에서 현재 애니로 전환되는 중
                progress = Mathf.Clamp01(nextInfo.normalizedTime);
            }
            else
            {
                var info = CharaterAnim.GetCurrentAnimatorStateInfo(0);
                
                //이전 애니가 재생된다거나, 상태 매니저를 거치지 않고 다른 애니가 재생될 경우 끝내기
                if (info.shortNameHash != _animationHash) 
                {
                    if(_hasStarted) IsFinished = true;
                    return;
                }
                _hasStarted = true;
                progress = Mathf.Clamp01(info.normalizedTime);
            }
            
            //대원_STUDY: if문으로 진행할 경우 프레임마다 실행하기 때문에 의도된 결과가 나오지 않음.
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

        
        private void SortEvents() //모든 startProgress를 한 곳에 모아 진행률 오름차순으로 정렬
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

            //  반환값이 음수 -> a가 b보다 앞에 위치
            //  반환값이 0 -> 순서유지
            //  반환값이 양수 -> b가 a보다 앞에 위치
            _sortedEvents.Sort((a, b) => a.Data.StartProgress.CompareTo(b.Data.StartProgress));
        }
    }
}