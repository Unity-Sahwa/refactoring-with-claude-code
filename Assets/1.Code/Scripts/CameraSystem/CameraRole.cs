using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 책임: 카메라 오브젝트에 붙어, 그 카메라의 용도를 표시한다.
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraRole : MonoBehaviour
    {
        [SerializeField] private CameraKind _kind;
        
        private CinemachineCamera _camera;

        public CameraKind Kind => _kind;
        public CinemachineCamera Camera => _camera != null ? _camera : (_camera = GetComponent<CinemachineCamera>());
    }
}
