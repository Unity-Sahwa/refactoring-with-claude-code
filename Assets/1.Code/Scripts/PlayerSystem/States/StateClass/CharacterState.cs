using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 한 애니메이션 상태의 공통 동작을 처리한다.
    //   - 진입 시 애니 재생 + 추적 시작
    //   - 매 프레임 진행률에 따라 이벤트(이펙트/히트박스/무적/스킬무브) 발행
    //   - 처리(how)는 직접 안 하고 이벤트로 외부에 위임한다
    // 개별 상태(IdleState 등)는 전환 규칙(TryHandleTrigger)만 채운다.
    // 클립명·이벤트 데이터는 머신이 SO로 주입한다(캐릭터별로 다름).
    public abstract class CharacterState
    {
        // 주입값(머신이 채움)
        public PlayerStateType StateKey { get; private set; }
        protected Animator Animator { get; private set; }
        protected ILockOnState LockOn { get; private set; }

        private IPlayerStateEventRaiser _raiser;
        private string _animationName;
        private int _animationHash;

        private readonly AnimationTracker _tracker = new AnimationTracker();
        private readonly List<(StateEventCategory Category, IStartData Data)> _sortedEvents = new();
        private int _readIndex;

        // 스왑 가능 여부(Idle/이동 등에서만 true). 머신이 스왑 입력 시 참조한다.
        public virtual bool CanSwap => false;

        // 애니 종료 여부. 머신이 보고 AnimationFinished 트리거를 자기에게 보낸다.
        public bool IsAnimationFinished => _tracker.IsFinished;

        // 루프 애니 상태(Locomotion 등)는 true → 머신이 애니 완료를 전환 신호로 쓰지 않는다.
        public virtual bool IsLooping => false;

        public void Initialize(PlayerStateType stateKey, Animator animator, ILockOnState lockOn,
                               IPlayerStateEventRaiser raiser, BaseStateData data, string animationName)
        {
            StateKey = stateKey;
            Animator = animator;
            LockOn = lockOn;
            _raiser = raiser;
            _animationName = animationName;
            _animationHash = Animator.StringToHash(animationName);

            SortEvents(data);
        }

        public virtual void Enter()
        {
            _readIndex = 0;
            _tracker.Begin(Animator, _animationHash);
            Animator.CrossFade(_animationName, 0.1f, 0, 0f);
        }

        // moveInput은 Locomotion 같은 blend 상태만 사용한다(나머지 상태는 무시).
        public virtual void Update()
        {
            _tracker.Update();

            float progress = _tracker.Progress;

            // 진행률을 넘긴 이벤트를 순서대로 발행(프레임당 여러 개 가능)
            while (_readIndex < _sortedEvents.Count
                   && progress >= _sortedEvents[_readIndex].Data.StartProgress)
            {
                (StateEventCategory category, IStartData data) = _sortedEvents[_readIndex];
                _raiser.Raise(category, data);
                _readIndex++;
            }
        }

        public virtual void Exit()
        {
            _readIndex = 0;
            _raiser.RaiseReset();
        }

        // 전환 규칙. 트리거에 반응해 다음 상태를 정한다. 전환 없으면 false.
        // 공통 전환(피격/사망/애니완료)은 여기서 처리하고, 개별 상태는 override해서 자기 전환만 더한다.
        public virtual bool TryHandleTrigger(StateTriggerType trigger, out PlayerStateType next)
        {
            switch (trigger)
            {
                case StateTriggerType.Damaged:
                    next = PlayerStateType.Hit;
                    return true;
                case StateTriggerType.Died:
                    next = PlayerStateType.Dead;
                    return true;
                case StateTriggerType.AnimationFinished:
                    next = PlayerStateType.Locomotion;
                    return true;
                default:
                    next = default;
                    return false;
            }
        }

        // 모든 진행률 이벤트를 한 곳에 모아 진행률 오름차순으로 정렬
        private void SortEvents(BaseStateData data)
        {
            _sortedEvents.Clear();
            if (data == null)
            {
                return;
            }

            Dictionary<StateEventCategory, IStartData[]> map = data.GetData<IStartData>();
            foreach ((StateEventCategory category, IStartData[] dataArray) in map)
            {
                foreach (IStartData entry in dataArray)
                {
                    _sortedEvents.Add((category, entry));
                }
            }

            _sortedEvents.Sort((a, b) => a.Data.StartProgress.CompareTo(b.Data.StartProgress));
        }
    }
}
