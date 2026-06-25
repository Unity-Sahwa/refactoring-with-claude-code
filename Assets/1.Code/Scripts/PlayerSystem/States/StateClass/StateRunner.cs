using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 상태를 관찰하고 무엇을 할지 신호(의도)를 보낸다.
    // 상태마다 클래스를 두지 않고, (PlayerStateType 키 + BaseStateData)로 초기화돼 각기 다른 상태처럼 동작하는 단일 실행기.
    public class StateRunner
    {
        // 주입값(머신이 채움)
        public PlayerStateType StateKey { get; private set; }
        protected PlayerCharacter Character { get; private set; }   // 이벤트 주체(어느 캐릭터가 발행했는지). 머신이 주입.
        protected Animator Animator { get; private set; }

        private IPlayerStateEventRaiser _raiser;
        private BaseStateData _data;   // 진입 시 재정렬(에디터 런타임 수정 반영)을 위해 보관
        private string _animationName;
        private int _animationHash;

        private readonly AnimationTracker _tracker = new AnimationTracker();
        // 진행률 시점마다 발행할 항목. 켜기(IsEnd=false)와 구간 끝(IsEnd=true)을 한 목록에 모아 진행률 순으로 발행.
        private readonly List<(float Progress, StateEventCategory Category, IStartData Data, bool IsEnd)> _sortedEvents = new();
        private int _readIndex;
        public bool IsAnimationFinished => _tracker.IsFinished; //상태머신이 보는 애니종료 여부
        public bool IsLooping => _data != null && _data.IsLooping;   // 루프 여부는 데이터(SO)가 가진다

        public void Initialize(PlayerStateType stateKey, PlayerCharacter character, Animator animator,
                               IPlayerStateEventRaiser raiser, BaseStateData data, string animationName)
        {
            StateKey = stateKey;
            Character = character;
            Animator = animator;
            _raiser = raiser;
            _animationName = animationName;
            _animationHash = Animator.StringToHash(animationName);

            _data = data;
            SortEvents(_data);
        }
        public void Enter()
        {
#if UNITY_EDITOR
            // 에디터 플레이모드에서 SO progress를 런타임 수정하면 다음 진입부터 반영되도록 재정렬한다.
            SortEvents(_data);
#endif
            _readIndex = 0;
            _tracker.Begin(Animator, _animationHash);
            Animator.CrossFade(_animationName, 0.1f, 0, 0f);
            _raiser.RaiseEnter(Character, StateKey);

            RaiseEvent(0f);
        }

        public void Update()
        {
            _tracker.Update();
            RaiseEvent(_tracker.Progress);
        }
        public void Exit()
        {
            _readIndex = 0;
            _raiser.RaiseReset();
        }

        private void RaiseEvent(float progress)
        {
            while (_readIndex < _sortedEvents.Count
                   && progress >= _sortedEvents[_readIndex].Progress)
            {
                (float _, StateEventCategory category, IStartData data, bool isEnd) = _sortedEvents[_readIndex];
                if (isEnd)
                {
                    _raiser.RaiseEnd(category);
                }
                else
                {
                    _raiser.Raise(Character, category, data);
                }
                _readIndex++;
            }
        }

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
                    // 켜기: 시작 진행률에 발행
                    _sortedEvents.Add((entry.StartProgress, category, entry, false));

                    // 끄기: 구간(IControlGate)이고 UntilEnd가 아니면 끝 진행률에 발행. UntilEnd면 Reset이 닫는다.
                    if (entry is IMotionControl gate && !gate.UntilEnd)
                    {
                        _sortedEvents.Add((entry.StartProgress + gate.Duration, category, entry, true));
                    }
                }
            }

            _sortedEvents.Sort((a, b) => a.Progress.CompareTo(b.Progress));
        }
    }
}
