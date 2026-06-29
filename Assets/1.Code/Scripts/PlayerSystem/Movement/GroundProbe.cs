using UnityEngine;

namespace Refactoring
{
    // 무엇: CharacterController에서 바닥과 충돌한 정보를 PlayerCharacterMover에게 전달 
    // 왜 별도 컴포넌트(캐릭터에 부착)인가: OnControllerColliderHit은 "CharacterController가 붙은 바로 그 GameObject의 스크립트"에서만 호출된다.
    // 왜 매 프레임 잘 갱신되나: 중력이 매 프레임 아래로 Move를 일으켜 경사면에 계속 닿으므로, 정지처럼 보여도 콜백이 호출된다.
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
