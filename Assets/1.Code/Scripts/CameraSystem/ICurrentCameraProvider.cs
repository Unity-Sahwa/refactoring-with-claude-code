using Unity.Cinemachine;

namespace Refactoring
{
    // 역할: 지금 실제로 화면을 그리고 있는(우선순위가 가장 높은) 가상 카메라를 조회만 제공한다(읽기 전용).
    public interface ICurrentCameraProvider
    {
        CinemachineCamera ActiveCamera { get; }
    }
}
