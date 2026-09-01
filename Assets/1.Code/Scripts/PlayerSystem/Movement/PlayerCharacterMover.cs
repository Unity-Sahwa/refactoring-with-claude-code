using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 역할: 매프레임 플레이어의 Transform 움직임(일반이동, 스킬이동, 넉백, 회전, 중력)을 호출
    // 기존에 분리되있던 이동을 왜 통합시켰나: 
    //      - 여러 곳에서 처리하면 꼬일까봐.
    //      - 이 컴포넌트가 Move의 유일한 호출자이고, "어떤 속도를 낼지"는 각 속도 소스(IVelocitySource)에 위임한다.
    //      - 이 컴포넌트는 씬의 유일한 DI 진입점이다. 주입받은 의존성을 순수 C# 소스들의 생성자로 넘겨 만들고,
    //        매 프레임 속도를 합산해 적용하며, 파괴 시 소스들의 구독을 Dispose로 정리한다.

    // 역할 분담:
    //   - Locomotion/Skill/Gravity Source : 각자 한 종류의 속도를 만든다(이벤트 구독·상태는 각자 관리).
    //   - CharacterRotator                : 회전(방향)만 담당.
    //   - GroundProbe(캐릭터에 부착)       : 발밑 면 법선을 받아 제공(가파른 경사 판정용).
    public class PlayerCharacterMover : MonoBehaviour
    {
        [Preserve, Inject] private ICurrentCharacterProvider _characterProvider;        // 필수: 움직일 대상(현재 캐릭터) 공급
        [Preserve, Inject(true)] private IInputMoveProvider _inputEventProvider;        // 옵션: 없으면 걷기, 회전만 빠짐(스킬, 중력은 동작)
        [Preserve, Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;  // 옵션: 없으면 스킬만 작동안함
        [Preserve, Inject(true)] private IStateTriggerRaiser _triggerRaiser;           // 옵션: 없으면 이동 시 Locomotion 전환 요청만 빠짐
        [Preserve, Inject(true)] private ICharacterSwapNotifier _swapNotifier;          // 옵션: 없으면 스왑 시 재획득 통지에만 사용
        [Preserve, Inject(true)] private ILockOnTarget _lockOnTarget;                   // 옵션: 없으면 회전은 항상 이동 방향

        //대원_TODO: 데이터 SO로 그룹화
        [Header("일반 이동")] 
        [SerializeField] private float _moveSpeed = 10f;   
        [SerializeField] private float _rotateRate = 20f;  
        
        [Header("추락(중력)")]
        [SerializeField] private float _gravity = -25f;    
        [SerializeField] private float _maxFallSpeed = -50f;
        [SerializeField] private float _groundedStick = -2f;  // 착지 시 바닥에 캐릭터를 살짝 눌러 붙이는 용도
        
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
            var frame = new MoveParams(Time.deltaTime, _characterTransform, _controller, groundNormal, _moveDirection);

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
            PlayerCharacter character = _characterProvider.CurrentCharacter;
            if (character == null)
            {
                Debug.LogError("[PlayerCharacterMover] 현재 캐릭터가 없습니다. Provider 설정을 확인하세요.");
                return;
            }

            _controller = character.GetCharacterComponent<CharacterController>();
            _characterTransform = character.transform;
            _groundProbe = character.GetCharacterComponent<GroundProbe>();
            _animator = character.GetComponentInChildren<Animator>(); // 히트스탑 판정용(HitStopHandler와 같은 탐색 방식)

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
