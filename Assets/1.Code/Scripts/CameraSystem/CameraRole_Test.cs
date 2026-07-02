using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    public enum CameraKind { Default, LockOn }

    // 카메라 오브젝트에 붙여 "평소용/락온용"을 표시한다.
    // Cinemachine 3.x에선 두 카메라가 같은 CinemachineCamera 타입이라, DI가 타입만으론 못 가린다.
    // 그래서 이 표식(kind)으로 A(CameraSwitcher_Test)가 둘을 구분한다.
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraRole_Test : MonoBehaviour
    {
        // 의존성 주입 대상이 아니라 "이 카메라의 역할" 설정값이라 SerializeField가 맞다.
        [SerializeField] private CameraKind kind;
        public CameraKind Kind => kind;

        private CinemachineCamera _camera;
        public CinemachineCamera Camera => _camera != null ? _camera : (_camera = GetComponent<CinemachineCamera>());
    }
}
