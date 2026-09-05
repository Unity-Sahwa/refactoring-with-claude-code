namespace Refactoring
{
    // 책임: PlayerCameraShakeHandler가 흔들기를 켜는 데 필요한 정보 계약.
    public interface IPlayerCameraShake
    {
        ShakeData Shake { get; }
    }
}
