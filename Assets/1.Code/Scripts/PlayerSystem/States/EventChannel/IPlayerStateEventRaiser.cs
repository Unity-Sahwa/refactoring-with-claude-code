namespace Refactoring
{
    public interface IPlayerStateEventRaiser
    {
        public void Raise(StateEventCategory categoryType, IStartData data);
        public void RaiseEnd(StateEventCategory categoryType);   // 구간의 끝(정상 종료) 시점 발행
        public void RaiseReset();                                // 상태 이탈 시 열린 것 전부 정리
    }
}
