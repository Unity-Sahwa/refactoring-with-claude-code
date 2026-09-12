using Unity.Cinemachine;

namespace Refactoring
{
    // 책임: 지금 화면을 그리고 있는(우선순위가 가장 높은) 가상 카메라를 읽기 전용으로 알려준다.
    public interface ICurrentCameraProvider
    {
        CinemachineCamera ActiveCamera { get; }
    }
}
