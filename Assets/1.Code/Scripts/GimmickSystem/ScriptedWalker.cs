using UnityEngine;

namespace Refactoring
{
    // 책임: 타임라인 연출 중 현재 캐릭터를 목표 지점까지 걸어가게 한다.
    // 왜 기존 이동 시스템을 안 쓰나: 컷씬 중엔 입력이 끊겨 WalkVelocitySource·CharacterRotator가 동작하지 않고,
    //                            상태 이벤트(MoveControl)에 따라 켜짐/꺼짐이 달라져 연출 결과가 들쭉날쭉해진다.
    //                            연출은 항상 같게 나와야 하므로 이동·회전·애니를 이 컴포넌트가 직접 잡는다.
    // 사용법: 타임라인 Signal Receiver에서 StartWalk/StopWalk를 호출한다.
    // ponytail: 중력은 PlayerCharacterMover가 계속 처리하므로 여기선 수평 이동만 한다.
    public class ScriptedWalker : MonoBehaviour
    {
        [Tooltip("캐릭터가 걸어갈 목표 지점")]
        [SerializeField]
        private Transform _goalPoint;

        [SerializeField]
        private float _moveSpeed = 3f;

        [SerializeField]
        private float _rotateRate = 10f;

        [Tooltip("목표까지 이 거리 안으로 들어오면 걷기를 멈춘다")]
        [SerializeField]
        private float _stopDistance = 0.2f;

        [Tooltip("연출 전용 걷기 애니메이션 스테이트 이름. 캐릭터 2개의 Animator에 같은 이름으로 있어야 한다")]
        [SerializeField]
        private string _walkStateName = "CutsceneWalk";

        [Tooltip("걷기가 끝나고 돌아갈 스테이트 이름")]
        [SerializeField]
        private string _idleStateName = "Locomotion";

        [SerializeField]
        private float _fadeTime = 0.2f;

        [Inject] private ICurrentCharacterProvider _characterProvider;

        private CharacterController _controller;
        private Transform _characterTransform;
        private Animator _animator;
        private bool _isWalking;

        private void Update()
        {
            if (!_isWalking)
            {
                return;
            }

            Vector3 toGoal = _goalPoint.position - _characterTransform.position;
            toGoal.y = 0f;

            if (toGoal.magnitude <= _stopDistance)
            {
                StopWalk();
                return;
            }

            Vector3 direction = toGoal.normalized;
            _controller.Move(direction * (_moveSpeed * Time.deltaTime));

            Quaternion look = Quaternion.LookRotation(direction);
            _characterTransform.rotation =
                Quaternion.Slerp(_characterTransform.rotation, look, Time.deltaTime * _rotateRate);
        }

        // 타임라인 Signal에서 호출. 지금 조작 중인 캐릭터를 잡아 목표 지점으로 걷기 시작한다.
        public void StartWalk()
        {
            PlayerCharacter character = _characterProvider?.CurrentCharacter;
            if (character == null || _goalPoint == null)
            {
                Debug.LogError("[ScriptedWalker] 현재 캐릭터 또는 목표 지점이 없습니다.");
                return;
            }

            _characterTransform = character.transform;
            _controller = character.GetCharacterComponent<CharacterController>();
            _animator = character.GetCharacterComponent<Animator>();
            _isWalking = true;
            // 상태머신 전이를 거치지 않고 연출용 스테이트를 바로 재생한다(입력 기반 이동 애니와 섞이지 않게).
            _animator?.CrossFade(_walkStateName, _fadeTime);
        }

        // 타임라인 Signal에서 호출. 목표에 먼저 도착하면 Update가 대신 호출한다.
        public void StopWalk()
        {
            _isWalking = false;
            _animator?.CrossFade(_idleStateName, _fadeTime);
        }
    }
}
