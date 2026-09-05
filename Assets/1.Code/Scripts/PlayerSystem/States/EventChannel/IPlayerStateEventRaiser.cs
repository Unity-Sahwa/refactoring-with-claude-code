namespace Refactoring
{
    // 책임: 상태가 기능 이벤트의 시작·끝·초기화를 발행하는 계약.
    public interface IPlayerStateEventRaiser
    {
        public void Raise(StateEventCategory categoryType, IStartData data);
        // 구간의 끝(정상 종료) 시점 발행
        public void RaiseEnd(StateEventCategory categoryType);

        // 상태 이탈 시 열린 것 전부 정리
        public void RaiseReset();
    }
}
