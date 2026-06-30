namespace Refactoring
{
    // 이동/회전 같은 물리 통제를 "이 구간 동안 허용한다"를 나타내는 데이터.
    // 시작은 진행률(StartProgress, IStartData)로 잡고, 지속은 Duration(진행률)으로 잰다.
    // UntilEnd가 true면 Duration을 무시하고 상태가 끝날 때(Reset)까지 허용을 유지한다.
    public interface IInputBlock
    {
        public float Duration {get;}   // 시작 진행률부터 허용을 유지하는 진행률 길이
        public bool UntilEnd {get;}     // true면 Duration 무시, Reset까지 허용 유지
    }
}
