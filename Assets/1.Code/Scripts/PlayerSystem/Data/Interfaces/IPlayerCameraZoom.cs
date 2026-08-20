namespace Refactoring
{
    // PlayerCameraZoomHandler가 상태 이벤트로부터 받을 데이터 타입
    public interface IPlayerCameraZoom
    {
        float DistanceScale { get; }  // 카메라 거리 배율(1보다 크면 멀어짐)
        float ZoomOutTime { get; }    // 배율에 도달하는 시간(초) — 짧게 주면 "확" 멀어짐
        float ZoomHoldTime { get; }   // 목표 배율을 유지하는 시간(초)
        float ZoomInTime { get; }     // 목표 배율에서 원래 거리로 복귀하는 시간(초)
    }
}
