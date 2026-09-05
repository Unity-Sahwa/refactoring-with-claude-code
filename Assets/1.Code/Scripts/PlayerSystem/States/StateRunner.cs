using System.Collections.Generic;
using UnityEngine;
namespace Refactoring
{
    // 책임: 애니메이션 진행률을 보고 그 시점의 상태 이벤트를 발행한다. (상태별 데이터는 StateData SO가 담당)
    // 흐름: Enter에서 애니 재생·이벤트 정렬 → Update마다 진행률 도달분 발행 → Exit에서 Reset 발행
    public class StateRunner
    {
        private readonly AnimationTracker _tracker = new AnimationTracker();
        private readonly List<(float Progress, StateEventCategory Category, IStartData Data, bool IsEnd)> _sortedEvents = new();
        private IPlayerStateEventRaiser _raiser;
        private StateData _data;
        private Animator _animator;
        private int _animationHash;
        private int _readIndex;
        private bool _exited;
        public PlayerStateType StateKey { get; private set; }
        public bool IsLooping { get; private set; }
        public bool IsAnimationFinished => _tracker.IsFinished;
        public float Cooldown => _data.Cooldown;
        public void Initialize(PlayerCharacter character, IPlayerStateEventRaiser raiser, StateData data)
        {
            StateKey = data.StateType;
            _raiser = raiser;
            _data = data;
            IsLooping = data.IsLooping;
            _animator = character.GetCharacterComponent<Animator>();
            _animationHash = Animator.StringToHash(StateKey.ToString());
            _tracker.Initialize(_animator, _animationHash);
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
            _animator.CrossFade(StateKey.ToString(), 0.1f, 0, 0f);
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
                if (_exited)
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
                    // 끄기: 구간이고 UntilEnd가 아니면 끝 진행률에 발행. UntilEnd면 Reset이 닫는다.
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