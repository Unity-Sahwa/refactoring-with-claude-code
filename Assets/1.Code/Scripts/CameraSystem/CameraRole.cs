using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 역할: 카메라 오브젝트에 붙어, 카메라 용도를 표시한다.
    // 컷씬·처형 카메라는 종류에 없다: 그건 타임라인 연출이 자기 카메라를 더 높은 우선순위로 직접 켠다.
    public enum CameraKind { Default, LockOn }

    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraRole : MonoBehaviour
    {
        [SerializeField] private CameraKind kind;
        public CameraKind Kind => kind;

        private CinemachineCamera _camera;
        public CinemachineCamera Camera => _camera != null ? _camera : (_camera = GetComponent<CinemachineCamera>());
    }
}
