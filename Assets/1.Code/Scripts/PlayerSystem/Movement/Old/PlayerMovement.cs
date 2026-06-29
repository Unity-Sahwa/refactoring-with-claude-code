using UnityEngine;

namespace Refactoring
{
    // 책임: 구독중인 이벤트 호출시 일정시간 동안 이동/회전이 가능하다.
    // 왜 이렇게 구현? : 책임을 명확이 분리하기 위해 플레이어 상태 시스템과 철저하게 분리. 이벤트로 작동
    // 왜 이렇게 구현? : 이동과 회전이 통제되는 구간이 달라 개별 처리함.
    // 흐름: 상태 이벤트 구독, 사용자 입력 구독 > 이동/회전 활성화 이벤트 받음> 사용자 입력을 받아 이동/회전 가능해짐 > 이동/회전 비활성화 이벤트 받음 > 이동/회전 불가능
    public class PlayerMovement : MonoBehaviour
    {
        [Inject] private IInputMoveProvider _inputEventProvider;
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        [Inject(true)] private IStateTriggerRaiser _triggerRaiser;
        [Inject(true)] private IPlayerSensor _sensor;

        private Rigidbody _rb;
        private CapsuleCollider _collider;
        private Transform _camera;
        private Vector3 _cameraForward;
        private Vector3 _cameraRight;
        private Vector2 _playerMoveVector = new Vector2();
        private Vector3 _movePosition;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _canMove;
        private bool _canRotate;
        private float _moveRate = 10; //대원_TODO : 플레이어 데이터로 옮기기
        private float _rotateRate = 20;

        void Awake()
        {
            _inputEventProvider.OnVector2Input += OnMove;
            _camera = Camera.main.transform;

            // 허용 구간 켜기/끄기와 상태 전환 Reset을 구독한다. 입력 구독과 달리 enabled로 막지 않는다(대부분 활성).
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Subscribe(StateEventCategory.MoveControl, HandleMoveOn);
                _eventSubscriber.SubscribeEnd(StateEventCategory.MoveControl, HandleMoveOff);
                _eventSubscriber.Subscribe(StateEventCategory.RotateControl, HandleRotateOn);
                _eventSubscriber.SubscribeEnd(StateEventCategory.RotateControl, HandleRotateOff);
                _eventSubscriber.SubscribeReset(HandleReset);
            }
        }
        void FixedUpdate()
        {
            if (_rb == null || _playerMoveVector == Vector2.zero)
            {
                return;
            }

            UpdateMoveDirection();

            if (_canMove)
            {
                Move();
                // 이동이 실제로 일어났으니 Locomotion 전환을 요청한다(이미 Locomotion이면 머신이 무시).
                _triggerRaiser?.RaiseTrigger(StateTriggerType.Move);
            }
            if (_canRotate)
            {
                Rotate();
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
                _eventSubscriber.UnsubscribeReset(HandleReset);
            }
        }

        private void OnMove(Vector2 vector2) => _playerMoveVector = vector2;
        private void HandleMoveOn(PlayerCharacter source, IStartData data)
        {
            SetRigidbody(source);
            _canMove = true;
        }
        private void HandleMoveOff() => _canMove = false;
        private void HandleRotateOn(PlayerCharacter source, IStartData data)
        {
            SetRigidbody(source);
            _canRotate = true;
        }
        private void HandleRotateOff() => _canRotate = false;
        // 상태 전환 시 호출. 허용이 다음 상태로 새지 않게 모두 끈다(UntilEnd 구간의 닫기도 여기서 보장).
        private void HandleReset()
        {
            _canMove = false;
            _canRotate = false;
        }
        private void SetRigidbody(PlayerCharacter source)
        {
            if (source != null)
            {
                _rb = source.GetCharacterComponent<Rigidbody>();
                _collider = source.GetCharacterComponent<CapsuleCollider>();
            }
        }
        private void UpdateMoveDirection()
        {
            //카메라가 바라보는 방향에서 위아래 방향을 제거한 노멀벡터
            _cameraForward.Set(_camera.forward.x,0,_camera.forward.z);
            _cameraForward.Normalize();
            _cameraRight.Set(_camera.right.x,0,_camera.right.z);
            _cameraRight.Normalize();

            //카메라기준 플레이어 좌우이동 + 플레이어 앞뒤이동
            _movePosition = _cameraRight * _playerMoveVector.x + _cameraForward * _playerMoveVector.y;
            _movePosition.Normalize();
        }
        private void Move()
        {
            _targetPosition = _rb.transform.position;
            Vector3 delta = _movePosition * Time.deltaTime * _moveRate;
            if (_sensor != null)
            {
                delta = _sensor.ResolveMove(_targetPosition, delta, _collider);
            }
            _rb.MovePosition(_targetPosition + delta);
        }
        private void Rotate()
        {
            _targetRotation = Quaternion.LookRotation(_movePosition);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, _targetRotation, Time.deltaTime * _rotateRate));
        }
    }
}
