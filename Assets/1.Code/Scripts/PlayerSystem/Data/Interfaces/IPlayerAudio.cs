namespace Refactoring
{
    // PlayerAudioHandler가 상태 이벤트로부터 받을 데이터 타입
    public interface IPlayerAudio
    {
        AudioId Id { get; }        // 재생할 소리 선택
        bool UntilFinish { get; }  // 상태 전환되도 끝까지 재생
    }
}
