using UnityEngine;

namespace Refactoring
{
    // ponytail: 프로토타입용 임시 구현. Camera.main을 그대로 쓴다.
    // A(카메라 전환) 시스템이 완성되면 "지금 켜져 있는 카메라"를 주는 구현으로 교체할 것.
    public class CameraPosition_Test : MonoBehaviour, ICameraPositionProvider
    {
        public Vector3 CameraPosition =>
            Camera.main != null ? Camera.main.transform.position : transform.position;
    }
}
