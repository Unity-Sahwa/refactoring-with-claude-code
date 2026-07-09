using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 상태를 관찰하고 무엇을 할지 신호(의도)를 보낸다.
    // 왜 상속구조 삭제?: 자식 클래스(상태)에 로직이 존재하지 않기 때문. 자식 클래스에 존재하는 데이터를 상태 데이터 SO에 포함시킴
    public class StateRunner
    {
        public PlayerStateType StateKey { get; private set; }
        protected PlayerCharacter Character { get; private set; }
        protected Animator Animator { get; private set; }

        private IPlayerStateEventRaiser _raiser;
        private StateData _data;   // 에디터용 변수
        private int _animationHash;

        private readonly AnimationTracker _tracker = new AnimationTracker();
        private readonly List<(float Progress, StateEventCategory Category, IStartData Data, bool IsEnd)> _sortedEvents = new();
        private int _readIndex;
        private bool _exited;
        public bool IsAnimationFinished => _tracker.IsFinished;
        public bool IsLooping {get; private set;}

        public void Initialize(PlayerCharacter character, IPlayerStateEventRaiser raiser, StateData data)
        {
            StateKey = data.StateType;
            _raiser = raiser;
            _data = data;
            IsLooping = data.IsLooping;
            Character = character;
            Animator = character.GetCharacterComponent<Animator>();
            _animationHash = Animator.StringToHash(StateKey.ToString());

            _tracker.Initialize(Animator, _animationHash);
            
            SortEvents(data);
        }
        public void Enter()
        {
#if UNITY_EDITOR
            // 에디터 플레이모드에서 SO progress를 런타임 수정하면 다음 진입부터 반영되도록 재정렬한다.
            SortEvents(_data);
            IsLooping = _data.IsLooping;
#endif
            _exited = false;
            _readIndex = 0;

            _tracker.Begin();

            Animator.CrossFade(StateKey.ToString(), 0.1f, 0, 0f);

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
            _exited = true;
            _raiser.RaiseReset();
        }
        private void RaiseEvent(float progress)
        {
            while (_readIndex < _sortedEvents.Count
                   && progress >= _sortedEvents[_readIndex].Progress)
            {
                if(_exited)
                {
                    return;
                }

                (float _, StateEventCategory category, IStartData data, bool isEnd) = _sortedEvents[_readIndex];
                
                if (isEnd)
                {
                    _raiser.RaiseEnd(category);
                }
                else
                {
                    _raiser.Raise(category, data);
                }
                _readIndex++;
            }
        }
        private void SortEvents(StateData data)
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
            //Sort는 리스트 내장 메서드. Sort의 매개변수는 T타입의 매개변수 2개를 가지는 delgate.
            //해당 delegate의 int 반환값을 Sort 메서드 내에서 사용하여 List의 정렬 진행
            _sortedEvents.Sort((a, b) => a.Progress.CompareTo(b.Progress));
        }
    }
}
