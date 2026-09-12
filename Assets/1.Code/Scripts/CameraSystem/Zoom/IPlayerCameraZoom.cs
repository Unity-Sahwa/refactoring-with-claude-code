namespace Refactoring
{
    // 책임: PlayerCameraZoomHandler가 줌을 켜는 데 필요한 정보 계약.
    public interface IPlayerCameraZoom
    {
        // 카메라 거리 배율(1보다 크면 멀어짐)
        float DistanceScale { get; }
        // 배율에 도달하는 시간(초) — 짧게 주면 "확" 멀어짐
        float ZoomOutTime { get; }
        // 목표 배율을 유지하는 시간(초)
        float ZoomHoldTime { get; }
        // 목표 배율에서 원래 거리로 복귀하는 시간(초)
        float ZoomInTime { get; }
    }
}
