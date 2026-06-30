using UnityEngine;

namespace Refactoring
{
    // 카메라의 월드 위치를 알려준다.
    // 탐지의 각도 계산이 "카메라에서 본 방향"을 기준으로 하기 때문에 카메라 위치가 필요하다.
    public interface ICameraPositionProvider
    {
        Vector3 CameraPosition { get; }
    }
}
