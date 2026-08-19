namespace Refactoring
{
    // PlayerCameraShakeHandler가 상태 이벤트로부터 받을 데이터 타입
    public interface IPlayerCameraShake
    {
        ShakeData Shake { get; }
    }
}
