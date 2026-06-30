using System;

namespace Refactoring
{
    public interface IPlayerStateEventRaiser
    {
        public void RaiseEnter(PlayerCharacter source, PlayerStateType state);
        public void Raise(PlayerCharacter source, StateEventCategory categoryType, IStartData data);
        public void RaiseEnd(StateEventCategory categoryType);   // 구간의 끝(끄기) 시점 발행
        public void RaiseReset();
        public void RaiseEnter(PlayerCharacter source, PlayerStateType state);
    }
}