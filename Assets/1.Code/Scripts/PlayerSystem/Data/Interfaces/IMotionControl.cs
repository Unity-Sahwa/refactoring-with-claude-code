namespace Refactoring
{
    // 책임: 이동·회전 통제를 "이 구간 동안 허용한다"로 나타내는 계약. (시작은 IStartData의 진행률이 잡는다)
    public interface IMotionControl
    {
        // 시작 진행률부터 허용을 유지하는 진행률 길이
        public float Duration {get;}
        // true면 Duration 무시, Reset까지 허용 유지
        public bool UntilEnd {get;}
    }
}
