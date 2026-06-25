using UnityEngine;

namespace Refactoring
{
    // 이동량을 지형에 맞게 보정하는 센서. 이동 컴포넌트가 MovePosition 직전에 호출한다.
    public interface IPlayerSensor
    {
        // 현재 위치·의도한 이동량·플레이어 콜라이더를 받아, 충돌을 반영해 보정한 이동량을 돌려준다.
        Vector3 ResolveMove(Vector3 currentPos, Vector3 intendedDelta, CapsuleCollider collider);
    }
}
