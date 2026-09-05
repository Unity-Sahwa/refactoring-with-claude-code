using UnityEngine;

namespace Refactoring
{
    // 책임: 발밑 면의 법선을 기록해 Mover에게 알린다.
    //       (OnControllerColliderHit은 CharacterController가 붙은 그 GameObject에서만 호출돼 별도 컴포넌트로 둔다)
    [RequireComponent(typeof(CharacterController))]
    public class GroundProbe : MonoBehaviour
    {
        // 가장 최근에 닿은 바닥/경사 면의 법선(Mover가 읽어 미끄러짐 판정에 사용).
        public Vector3 GroundNormal { get; private set; } = Vector3.up;

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // 노말벡터의 y값이 0 초과라는 것은 위를 바라본다는 뜻
            // 바닥, 경사일때만 기록한다.
            if (hit.normal.y > 0f)
            {
                GroundNormal = hit.normal;
            }
        }
    }
}
