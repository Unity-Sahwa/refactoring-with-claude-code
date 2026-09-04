using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 구독중인 이벤트 호출시 일정시간 동안 방향키 입력을 애니메이션 파라미터로 반영한다.
    // 왜 이렇게 구현? : PlayerMovement와 동일하게 플레이어 상태 시스템과 분리. 이벤트로 On/Off만 받는다.
    // 흐름: 상태 이벤트 구독, 사용자 입력 구독 > 파라미터 갱신 활성화 이벤트 받음 > 입력을 받아 파라미터 갱신 > 비활성화 이벤트 받음 > 갱신 중지
    public class PlayerMoveAnimation : MonoBehaviour
    {
        [Preserve, Inject] private IInputMoveProvider _inputEventProvider;
        [Preserve, Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        [Preserve, Inject(true)] private ILockOnState _lockOnState;
        [Preserve, Inject(true)] private ICurrentCharacterProvider _currentCharacterProvider;   // 현재 캐릭터의 Animator 조회용

        private Animator _animator;
        private Vector2 _playerMoveVector = new Vector2();
        private IDisposable _moveEventDisposable;
        private bool _canSetParameter;

        private readonly int _moveXHash = Animator.StringToHash("MoveX");
        private readonly int _moveYHash = Animator.StringToHash("MoveY");
        private float _dampTime = 0.1f;

        void Awake()
        {
            _inputEventProvider.OnVector2Input += OnMove;

            if (_eventSubscriber != null)
            {
                _moveEventDisposable = _eventSubscriber.Register(StateEventCategory.MoveControl, HandleOn, HandleClose);
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
            _moveEventDisposable?.Dispose();
        }

        private void OnMove(Vector2 vector2) => _playerMoveVector = vector2;
        private void HandleOn(IStartData data)
        {
            SetAnimator(_currentCharacterProvider?.CurrentCharacter);
            _canSetParameter = true;
        }
        
        private void HandleClose(CloseEventType reason)
        {
            _canSetParameter = false;
            if (reason == CloseEventType.End)
            {
                _animator.SetFloat(_moveYHash, 0f);
            }
        }
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
