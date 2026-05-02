using System;
using UnityEngine;

// Trigger 콜백 제거 (설계 결정 4: 히트박스 충돌은 HitBoxComponent가 담당)
// IPlayerContext 없음 (설계 결정 1: 외부 시스템과 통신은 event/Action으로만)
namespace Refactoring
{
    // 제네릭 Enum 기반 상태 클래스. StateKey와 CanReenter 속성을 포함한다.

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
    
    public abstract class BaseState<EState> where EState : Enum
    {
        public EState StateKey { get; protected set; }

        // false이면 동일 상태 재진입 요청 무시
        public bool CanReenter { get; protected set; } = false;

        protected IStateContext Manager { get;}

        protected IStateDataProvider DataProvider { get; }

        protected Animation characterAnimation {get;}

        protected BaseState(IStateContext manager)
        {
            Manager = manager;
        } 

        public abstract void EnterState();
        
        public abstract void UpdateState();

        public abstract void ExitState();
    }
}