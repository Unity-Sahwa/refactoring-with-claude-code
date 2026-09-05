using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: CharacterController.Move의 유일한 호출자. (속도는 각 IVelocitySource가, 회전은 CharacterRotator가 만든다)
    // 흐름: Awake에서 속도 소스 생성 → 매 프레임 속도 합산·회전 적용 → Move 한 번 호출 → 파괴 시 소스 Dispose
    public class PlayerCharacterMover : MonoBehaviour
    {
        // 필수: 움직일 대상(현재 캐릭터) 공급
        [Preserve, Inject] private ICurrentCharacterProvider _characterProvider;
        // 옵션: 없으면 걷기, 회전만 빠짐(스킬, 중력은 동작)
        [Preserve, Inject(true)] private IInputMoveProvider _inputEventProvider;
        // 옵션: 없으면 스킬만 작동안함
        [Preserve, Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        // 옵션: 없으면 이동 시 Locomotion 전환 요청만 빠짐
        [Preserve, Inject(true)] private IStateTriggerRaiser _triggerRaiser;
        // 옵션: 없으면 스왑 시 재획득 통지에만 사용
        [Preserve, Inject(true)] private ICharacterSwapNotifier _swapNotifier;
        // 옵션: 없으면 회전은 항상 이동 방향
        [Preserve, Inject(true)] private ILockOnTarget _lockOnTarget;

        [Header("일반 이동")]
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _rotateRate = 20f;

        [Header("추락(중력)")]
        [SerializeField] private float _gravity = -25f;
        [SerializeField] private float _maxFallSpeed = -50f;
        [Tooltip("착지 시 바닥에 캐릭터를 살짝 눌러 붙이는 속도")]
        [SerializeField] private float _groundedStick = -2f;

        private readonly List<IVelocitySource> _velocitySources = new();
        private CharacterRotator _rotator;
        private CharacterController _controller;
        private Transform _characterTransform;
        private GroundProbe _groundProbe;
        private Animator _animator;

        private Vector2 _inputVector;
        private Transform _camera;
        private Vector3 _cameraForward;
        private Vector3 _cameraRight;
        private Vector3 _moveDirection;

        private void Awake()
        {
            if (_inputEventProvider != null)
            {
                _inputEventProvider.OnVector2Input += OnMove;
            }
            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped += SetupCurrentCharacter;
            }
            _camera = Camera.main != null ? Camera.main.transform : null;

            _velocitySources.Add(new WalkVelocitySource(_eventSubscriber, _triggerRaiser, _moveSpeed));
            _velocitySources.Add(new SkillVelocitySource(_eventSubscriber));
            _velocitySources.Add(new GravityVelocitySource(_gravity, _maxFallSpeed, _groundedStick));
            _rotator = new CharacterRotator(_eventSubscriber, _rotateRate, _lockOnTarget);
        }

        private void Start()
        {
            SetupCurrentCharacter();
        }

        private void OnDestroy()
        {
            if (_inputEventProvider != null)
            {
                _inputEventProvider.OnVector2Input -= OnMove;
            }
            if (_swapNotifier != null)
            {
                _swapNotifier.OnCharacterSwapped -= SetupCurrentCharacter;
            }

            for (int i = 0; i < _velocitySources.Count; i++)
            {
                _velocitySources[i].Dispose();
            }
            _rotator?.Dispose();
        }

        private void Update()
        {
            if (_controller == null || _characterTransform == null)
            {
                return;
            }

            // 히트스탑(HitStopHandler가 anim.speed=0으로 애니만 정지) 동안에는 이동 계산을 통째로 건너뛴다.
            // 안 그러면 애니는 멈춘 채 SkillVelocitySource의 elapsed만 흘러서, 스탑이 끝났을 땐 스킬 이동 구간이
            // 이미 소진돼 있다(= 스킬무브가 짧게 끊기거나 아예 안 나가는 증상).
            // ponytail: 스탑 동안 중력/걷기도 같이 멈춘다. 히트스탑은 곧 시간 정지라 의도된 동작.
            if (_animator != null && _animator.speed == 0f)
            {
                return;
            }

            UpdateInputMoveDirection();

            Vector3 groundNormal = _groundProbe != null ? _groundProbe.GroundNormal : Vector3.up;
            MoveParams frame = new MoveParams(Time.deltaTime, _characterTransform, _controller, groundNormal, _moveDirection);

            Vector3 velocity = Vector3.zero;
            for (int i = 0; i < _velocitySources.Count; i++)
            {
                velocity += _velocitySources[i].Evaluate(in frame);
            }

            _rotator.Apply(in frame);
            // 합친 속도를 거리로 바꿔 한 번만 이동. 충돌·관통 방지는 CharacterController가 처리.
            _controller.Move(velocity * frame.DeltaTime);
        }

        private void OnMove(Vector2 vector2)
        {
            _inputVector = vector2;
        }

        private void SetupCurrentCharacter()
        {
            _characterTransform = _characterProvider.GetCurrentComponent<Transform>();
            if (_characterTransform == null)
            {
                Debug.LogError("[PlayerCharacterMover] 현재 캐릭터가 없습니다. Provider 설정을 확인하세요.");
                return;
            }

            _controller = _characterProvider.GetCurrentComponent<CharacterController>();
            _groundProbe = _characterProvider.GetCurrentComponent<GroundProbe>();
            // 히트스탑 판정용(HitStopHandler와 같은 탐색 방식)
            _animator = _characterTransform.GetComponentInChildren<Animator>();

            // 캐릭터가 바뀌었으니 소스들의 누적 상태(예: 낙하속도)를 초기화한다.
            for (int i = 0; i < _velocitySources.Count; i++)
            {
                _velocitySources[i].OnCharacterChanged();
            }
        }

        // 카메라가 보는 방향(위아래 제거) 기준으로 입력을 수평 이동 방향으로 바꾼다.
        private void UpdateInputMoveDirection()
        {
            if (_camera == null || _inputVector == Vector2.zero)
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
