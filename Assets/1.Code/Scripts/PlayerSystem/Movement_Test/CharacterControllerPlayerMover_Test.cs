using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 무엇: 기존 PlayerMovement(입력 이동·회전) + PlayerSkillMoveHandler(상태가 주는 스킬 이동) + 추락을
    //       CharacterController 위에서 "실제 입력·상태 이벤트·스킬 데이터" 그대로 재현하는 테스트 컴포넌트.
    //
    // 왜 이렇게 만들었나:
    //   - 기존 Rigidbody+MovePosition 방식은 그대로 두고, CharacterController로 바꾸면 "뚫림"이 사라지는지
    //     실제 플레이(상태 이벤트·스킬 데이터)와 똑같은 조건에서 검증하기 위함이다.
    //   - 특히 우리가 겪는 뚫림 두 가지를 그대로 재현해야 한다:
    //       (1) 모서리(90도 안쪽 코너)에서 돌진 스킬   (2) 회전낙법처럼 아래로 큰 속력을 내는 스킬
    //     이 둘은 상태가 주는 ISkillMove(Direction·Speed·Duration) 데이터로 발동되므로, 그 데이터를 그대로 쓴다.
    //
    // CharacterController로 합치는 이유:
    //   - CharacterController.Move는 "이만큼 가라"를 주면 그 거리를 훑어(sweep) 벽에 닿으면 멈춘다(빠른 이동 관통 방지).
    //   - 그래서 일반이동·스킬이동·추락을 모두 "이번 프레임 속도"로 한 벡터에 더해 Move 한 번에 넘긴다.
    //   - Move는 한 곳에서만 불러야 하므로(여러 번 부르면 따로 처리돼 꼬임) 이 컴포넌트 하나가 전담한다.
    //
    // 씬 테스트 준비:
    //   - 캐릭터(H·A) 오브젝트에 CharacterController를 추가하고 Radius/Height/Center를 캡슐에 맞춘다.
    //   - 이 컴포넌트는 기존 매니저 오브젝트(PlayerMovement가 있던 곳)에 두면, DI로 입력·상태·현재캐릭터가 주입된다.
    //   - 기존 PlayerMovement / PlayerSkillMoveHandler 는 같은 캐릭터를 두 번 움직이지 않도록 꺼두고 테스트한다.
    //     대원_TODO: 기존 두 컴포넌트와 동시 작동하지 않게(중복 이동 방지) 씬에서 켜고 끄는 것을 잊지 말 것.
    public class CharacterControllerPlayerMover_Test : MonoBehaviour
    {
        [Inject] private IInputMoveProvider _inputEventProvider;
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        [Inject(true)] private ICharacterSwapNotifier _swapNotifier;

        [Header("일반 이동")]
        [SerializeField] private float _moveSpeed = 10f;   // 기존 PlayerMovement의 _moveRate에 해당
        [SerializeField] private float _rotateRate = 20f;  // 기존 PlayerMovement의 _rotateRate에 해당

        [Header("추락(중력)")]
        [SerializeField] private float _gravity = -25f;       // 직접 내려주는 중력 가속도(음수)
        [SerializeField] private float _maxFallSpeed = -50f;  // 최대 낙하 속도
        [SerializeField] private float _groundedStick = -2f;  // 착지 시 바닥에 살짝 눌러 붙이는 속도

        [Header("디버그")]
        [SerializeField] private bool _debugLog = true;

        // 현재(스왑된) 캐릭터의 CharacterController와 Transform
        private CharacterController _controller;
        private Transform _characterTransform;
        private ControllerHitLogger_Test _hitProvider; // 캐릭터에 붙은 충돌 로거(닿은 면 법선 제공)

        // 입력/상태 플래그 (기존 PlayerMovement와 동일한 의미)
        private Vector2 _inputVector;
        private bool _canMove;
        private bool _canRotate;

        private Transform _camera;
        private Vector3 _cameraForward;
        private Vector3 _cameraRight;
        private Vector3 _moveDirection;

        private float _verticalSpeed;

        // 발밑에 닿은 면의 법선(경사 각도 판정용). 캐릭터의 충돌 로거(OnControllerColliderHit)에서 받아온다.
        private Vector3 _groundNormal = Vector3.up;

        // 진행 중인 스킬 이동들(기존 SkillMoveHandler와 동일하게 다중 합산)
        private readonly List<ActiveSkillMove> _skillMoves = new();
        private class ActiveSkillMove
        {
            public Vector3 localVelocity; // Direction.normalized * Speed (캐릭터 로컬 기준)
            public float duration;
            public float elapsed;
        }

        private void Awake()
        {
            _inputEventProvider.OnVector2Input += OnMove;
            _camera = Camera.main != null ? Camera.main.transform : null;

            if (_eventSubscriber != null)
            {
                _eventSubscriber.Subscribe(StateEventCategory.MoveControl, HandleMoveOn);
                _eventSubscriber.SubscribeEnd(StateEventCategory.MoveControl, HandleMoveOff);
                _eventSubscriber.Subscribe(StateEventCategory.RotateControl, HandleRotateOn);
                _eventSubscriber.SubscribeEnd(StateEventCategory.RotateControl, HandleRotateOff);
                _eventSubscriber.Subscribe(StateEventCategory.SkillMove, HandleSkillMove);
                _eventSubscriber.SubscribeReset(HandleReset);
            }
        }

        private void Start()
        {
            AcquireCurrentCharacter();
            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped += OnCharacterSwapped;
            }
        }

        private void OnDestroy()
        {
            if (_inputEventProvider != null)
            {
                _inputEventProvider.OnVector2Input -= OnMove;
            }
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe(StateEventCategory.MoveControl, HandleMoveOn);
                _eventSubscriber.UnsubscribeEnd(StateEventCategory.MoveControl, HandleMoveOff);
                _eventSubscriber.Unsubscribe(StateEventCategory.RotateControl, HandleRotateOn);
                _eventSubscriber.UnsubscribeEnd(StateEventCategory.RotateControl, HandleRotateOff);
                _eventSubscriber.Unsubscribe(StateEventCategory.SkillMove, HandleSkillMove);
                _eventSubscriber.UnsubscribeReset(HandleReset);
            }
            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped -= OnCharacterSwapped;
            }
        }

        private void OnCharacterSwapped(PlayerCharacterType type) => AcquireCurrentCharacter();

        // 현재 캐릭터의 CharacterController·Transform을 잡는다(스왑되면 다시 잡음).
        private void AcquireCurrentCharacter()
        {
            GameObject go = _swapNotifier?.CurrentCharacterObject;
            if (go == null)
            {
                _controller = null;
                _characterTransform = null;
                return;
            }
            _controller = go.GetComponent<CharacterController>();
            _characterTransform = go.transform;
            _hitProvider = go.GetComponent<ControllerHitLogger_Test>();
            _verticalSpeed = 0f;
        }

        // 이동은 매 프레임 한 번, 모든 속도를 합쳐 CharacterController.Move로 적용한다.
        // (CharacterController는 Update 타이밍 사용이 일반적이다)
        private void Update()
        {
            if (_controller == null || _characterTransform == null)
            {
                return;
            }

            float dt = Time.deltaTime;

            // 캐릭터에 붙은 충돌 로거가 OnControllerColliderHit으로 받아둔 면 법선을 가져온다.
            // (중력이 매 프레임 아래로 Move를 일으켜 경사면에 계속 닿으므로, 정지처럼 보여도 법선이 갱신된다)
            _groundNormal = _hitProvider != null ? _hitProvider.GroundNormal : Vector3.up;

            Vector3 velocity = Vector3.zero;

            // 1) 일반 수평 이동(허용 상태일 때만). 기존 PlayerMovement와 동일하게 카메라 기준 방향.
            UpdateMoveDirection();
            if (_canMove && _inputVector != Vector2.zero)
            {
                velocity += _moveDirection * _moveSpeed;
            }

            // 2) 회전(허용 상태일 때만). CharacterController는 안 돌므로 캐릭터 Transform을 돌린다.
            if (_canRotate && _inputVector != Vector2.zero)
            {
                Quaternion target = Quaternion.LookRotation(_moveDirection);
                _characterTransform.rotation = Quaternion.Slerp(_characterTransform.rotation, target, dt * _rotateRate);
            }

            // 3) 스킬 이동(다중 합산 + 수명 처리). 캐릭터가 바라보는 방향으로 회전시켜 더한다.
            velocity += ConsumeSkillVelocity(dt);

            // 4) 추락(중력) + 가파른 경사 미끄러짐.
            //    CharacterController는 Slope Limit 초과 경사에서 "올라가기"만 막을 뿐, 미끄러뜨리진 않는다.
            //    그래서 발밑 면이 한계각보다 가파르면(=서있으면 안 되는 경사) 중력을 그 면을 따라 흐르게 해서 직접 미끄러뜨린다.
            // 각도만으로 가파른 경사 판정(isGrounded 제외 → 벽 옆구리에 닿아 isGrounded가 false여도 미끄러짐 유지).
            float groundAngle = Vector3.Angle(_groundNormal, Vector3.up);
            bool onSteepSlope = groundAngle > _controller.slopeLimit;

            // 중력은 항상 아래로 쌓는다.
            _verticalSpeed += _gravity * dt;
            _verticalSpeed = Mathf.Max(_verticalSpeed, _maxFallSpeed);

            // 걸을 수 있는 바닥에 착지했으면 떨어지는 속도를 멈춰 바닥에 붙인다(가파른 경사는 제외 → 계속 미끄러지게).
            if (_controller.isGrounded && !onSteepSlope && _verticalSpeed < 0f)
            {
                _verticalSpeed = _groundedStick;
            }

            if (onSteepSlope)
            {
                // 가파른 경사: 쌓인 중력 속도를 경사면을 따라 흐르게 투영 → 면을 타고 미끄러져 내려간다.
                velocity += Vector3.ProjectOnPlane(new Vector3(0f, _verticalSpeed, 0f), _groundNormal);
            }
            else
            {
                velocity.y += _verticalSpeed;
            }

            // 5) 합친 속도를 거리로 바꿔 한 번만 이동. 충돌·관통 방지는 CharacterController가 처리.
            _controller.Move(velocity * dt);

            // [벽타기감지] 디버그는 잠시 끔(주석)
            //Vector3 beforePos = _characterTransform.position; // Move 전에 위치 저장해야 함
            //if (_debugLog)
            //{
            //    float wantedDy = velocity.y * dt;
            //    float actualDy = _characterTransform.position.y - beforePos.y;
            //    if (actualDy > wantedDy + 0.002f)
            //    {
            //        Debug.Log($"[벽타기감지] 의도dy={wantedDy:F3} 실제dy={actualDy:F3} | steep={onSteepSlope} groundAngle={groundAngle:F1} grounded={_controller.isGrounded} normalY={_groundNormal.y:F2} | skill={_skillMoves.Count} canMove={_canMove} velY={velocity.y:F2} velXZ={new Vector3(velocity.x,0,velocity.z).magnitude:F2}");
            //    }
            //}

            // [isGrounded] 디버그는 잠시 끔(주석)
            //if (_debugLog && _controller.isGrounded != _prevGrounded)
            //{
            //    Debug.Log($"[isGrounded] {_controller.isGrounded}");
            //    _prevGrounded = _controller.isGrounded;
            //}
        }

        // CharacterController의 실제 충돌 캡슐을 Scene 뷰에 그린다(초록 캡슐=충돌체, 자홍 점=피벗 위치).
        private void OnDrawGizmos()
        {
            if (_controller == null || _characterTransform == null)
            {
                return;
            }

            Vector3 scale = _characterTransform.lossyScale;
            float rScaled = _controller.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float hScaled = _controller.height * Mathf.Abs(scale.y);
            float half = Mathf.Max(0f, hScaled * 0.5f - rScaled);
            Vector3 center = _characterTransform.TransformPoint(_controller.center);
            Vector3 capTop = center + Vector3.up * half;
            Vector3 capBottom = center - Vector3.up * half;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(capTop, rScaled);
            Gizmos.DrawWireSphere(capBottom, rScaled);
            Gizmos.DrawLine(capTop + Vector3.right * rScaled, capBottom + Vector3.right * rScaled);
            Gizmos.DrawLine(capTop - Vector3.right * rScaled, capBottom - Vector3.right * rScaled);
            Gizmos.DrawLine(capTop + Vector3.forward * rScaled, capBottom + Vector3.forward * rScaled);
            Gizmos.DrawLine(capTop - Vector3.forward * rScaled, capBottom - Vector3.forward * rScaled);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_characterTransform.position, 0.05f);
        }

        // (참고) 경사 법선은 캐릭터에 붙은 ControllerHitLogger_Test가 OnControllerColliderHit으로 받아 GroundNormal로 제공한다.
        //  Mover는 매니저 오브젝트라 콜백을 직접 못 받으므로, AcquireCurrentCharacter에서 잡아둔 _hitProvider에서 읽어 쓴다.

        // 진행 중인 스킬 이동들의 이번 프레임 속도를 합산하고 수명을 깎는다(기존 SkillMoveHandler 로직 재현).
        private Vector3 ConsumeSkillVelocity(float dt)
        {
            if (_skillMoves.Count == 0)
            {
                return Vector3.zero;
            }

            // 역순 순회로 만료된 것 제거
            for (int i = _skillMoves.Count - 1; i >= 0; i--)
            {
                _skillMoves[i].elapsed += dt;
                if (_skillMoves[i].elapsed >= _skillMoves[i].duration)
                {
                    _skillMoves.RemoveAt(i);
                }
            }

            Vector3 worldVelocity = Vector3.zero;
            for (int i = 0; i < _skillMoves.Count; i++)
            {
                // 로컬 방향 속도를 캐릭터가 바라보는 방향으로 돌려서 더한다.
                worldVelocity += _characterTransform.rotation * _skillMoves[i].localVelocity;
            }
            return worldVelocity;
        }

        private void OnMove(Vector2 vector2) => _inputVector = vector2;

        private void HandleMoveOn(PlayerCharacter source, IStartData data) => _canMove = true;
        private void HandleMoveOff() => _canMove = false;
        private void HandleRotateOn(PlayerCharacter source, IStartData data) => _canRotate = true;
        private void HandleRotateOff() => _canRotate = false;

        // 상태가 스킬 이동을 켤 때 호출. ISkillMove 데이터(방향·속도·시간) 그대로 활성 목록에 추가한다.
        private void HandleSkillMove(PlayerCharacter source, IStartData data)
        {
            if (data is not ISkillMove skillMoveData)
            {
                Debug.LogError($"[CharacterControllerPlayerMover_Test] ISkillMove가 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            _skillMoves.Add(new ActiveSkillMove
            {
                localVelocity = skillMoveData.Direction.normalized * skillMoveData.Speed,
                duration = skillMoveData.Duration,
                elapsed = 0f
            });
        }

        // 상태 전환 시 모든 허용·진행 이동을 끈다(다음 상태로 새지 않게).
        private void HandleReset()
        {
            _canMove = false;
            _canRotate = false;
            _skillMoves.Clear();
        }

        // 카메라가 보는 방향(위아래 제거) 기준으로 입력을 수평 이동 방향으로 바꾼다.
        private void UpdateMoveDirection()
        {
            if (_camera == null)
            {
                _moveDirection = Vector3.zero;
                return;
            }
            _cameraForward.Set(_camera.forward.x, 0, _camera.forward.z);
            _cameraForward.Normalize();
            _cameraRight.Set(_camera.right.x, 0, _camera.right.z);
            _cameraRight.Normalize();

            _moveDirection = _cameraRight * _inputVector.x + _cameraForward * _inputVector.y;
            _moveDirection.Normalize();
        }
    }
}
