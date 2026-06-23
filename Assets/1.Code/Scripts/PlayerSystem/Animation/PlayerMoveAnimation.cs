using UnityEngine;

namespace Refactoring
{
    // 책임: 구독중인 이벤트 호출시 일정시간 동안 방향키 입력을 애니메이션 파라미터로 반영한다.
    // 왜 이렇게 구현? : PlayerMovement와 동일하게 플레이어 상태 시스템과 분리. 이벤트로 On/Off만 받는다.
    // 흐름: 상태 이벤트 구독, 사용자 입력 구독 > 파라미터 갱신 활성화 이벤트 받음 > 입력을 받아 파라미터 갱신 > 비활성화 이벤트 받음 > 갱신 중지
    public class PlayerMoveAnimation : MonoBehaviour
    {
        [Inject] private IInputMoveProvider _inputEventProvider;
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        [Inject(true)] private ILockOnState _lockOnState;

        private Animator _animator;
        private Vector2 _playerMoveVector = new Vector2();
        private bool _canSetParameter;

        // 대원_TODO : 파라미터 이름 플레이어 데이터로 옮기기
        private readonly int _moveXHash = Animator.StringToHash("MoveX");
        private readonly int _moveYHash = Animator.StringToHash("MoveY");
        private float _dampTime = 0.1f;

        void Awake()
        {
            _inputEventProvider.OnVector2Input += OnMove;

            // 허용 구간 켜기/끄기와 상태 전환 Reset을 구독한다.
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Subscribe(StateEventCategory.MoveControl, HandleOn);
                _eventSubscriber.SubscribeEnd(StateEventCategory.MoveControl, HandleOff);
                _eventSubscriber.SubscribeReset(HandleReset);
            }
        }
        void Update()
        {
            if (_animator == null || !_canSetParameter)
            {
                return;
            }

            SetParameter();
        }
        private void OnDestroy()
        {
            if (_inputEventProvider != null)
            {
                _inputEventProvider.OnVector2Input -= OnMove;
            }
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe(StateEventCategory.MoveControl, HandleOn);
                _eventSubscriber.UnsubscribeEnd(StateEventCategory.MoveControl, HandleOff);
                _eventSubscriber.UnsubscribeReset(HandleReset);
            }
        }

        private void OnMove(Vector2 vector2) => _playerMoveVector = vector2;
        private void HandleOn(PlayerCharacter source, IStartData data)
        {
            SetAnimator(source);
            _canSetParameter = true;
        }
        private void HandleOff()
        {
            _canSetParameter = false;
            //_animator.SetFloat(_moveXHash, 0f);
            _animator.SetFloat(_moveYHash, 0f);
            
        }
        // 상태 전환 시 호출. 허용이 다음 상태로 새지 않게 끈다.
        private void HandleReset() => _canSetParameter = false;
        private void SetAnimator(PlayerCharacter source)
        {
            if (source != null)
            {
                _animator = source.GetCharacterComponent<Animator>();
            }
        }
        private void SetParameter()
        {
            if (_lockOnState != null && _lockOnState.IsLockOn)
            {
                // 락온: 타깃 기준 전후좌우 방향 블렌드(MoveX·MoveY 둘 다 사용)
                _animator.SetFloat(_moveXHash, _playerMoveVector.x, _dampTime, Time.deltaTime);
                _animator.SetFloat(_moveYHash, _playerMoveVector.y, _dampTime, Time.deltaTime);
            }
            else
            {
                // 비락온: 진행 속력만(MoveY), MoveX는 0
                _animator.SetFloat(_moveXHash, 0f, _dampTime, Time.deltaTime);
                _animator.SetFloat(_moveYHash, _playerMoveVector.magnitude, _dampTime, Time.deltaTime);
            }
        }
    }
}
