// using UnityEngine;

// namespace Refactoring
// {
//     // 무엇: CharacterController가 닿은 "바닥/경사 면의 법선"을 보관해, 미끄러짐 판정에 쓸 수 있게 제공한다(+면 각도 로그).
//     //
//     // 왜 별도 클래스인가:
//     //   OnControllerColliderHit은 "CharacterController가 붙어 있는 바로 그 GameObject의 스크립트"에서만 호출된다.
//     //   이동 로직(Mover)은 매니저 오브젝트에 있어 이 콜백을 못 받으므로, 캐릭터 오브젝트에 붙는 이 컴포넌트가 대신 받아
//     //   GroundNormal로 들고 있고, Mover가 그 값을 읽어 가파른 경사 미끄러짐에 사용한다.
//     //
//     // 왜 매 프레임 잘 갱신되나: 중력이 매 프레임 아래로 Move를 일으켜 경사면에 계속 닿으므로, 정지처럼 보여도 콜백이 호출된다.
//     //
//     // 사용법: CharacterController가 붙은 캐릭터(H·A) 오브젝트에 이 컴포넌트를 붙인다.
//     [RequireComponent(typeof(CharacterController))]
//     public class ControllerHitLogger_Test : MonoBehaviour
//     {
//         [SerializeField] private bool _logEnabled = true;

//         // 가장 최근에 닿은 바닥/경사 면의 법선(Mover가 읽어 미끄러짐 판정에 사용).
//         public Vector3 GroundNormal { get; private set; } = Vector3.up;

//         private void OnControllerColliderHit(ControllerColliderHit hit)
//         {
//             // 아래를 향하는 면(바닥·경사)만 기록한다. 벽(법선 수평)·천장은 미끄러짐 판정에서 제외.
//             if (hit.normal.y > 0f)
//             {
//                 GroundNormal = hit.normal;
//             }

//             if (_logEnabled)
//             {
//                 Debug.Log($"[면각도] {Vector3.Angle(hit.normal, Vector3.up):F1} (hit={hit.collider.name})");
//             }
//         }
//     }
// }
