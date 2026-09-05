namespace Refactoring
{
    // 책임: PlayerAudioHandler가 소리를 켜는 데 필요한 정보 계약.
    public interface IPlayerAudio
    {
        // 재생할 소리 선택
        SoundType Id { get; }
        // 상태 전환되도 끝까지 재생
        bool UntilFinish { get; }
    }
}
