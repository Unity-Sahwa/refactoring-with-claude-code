using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 사용자 입력에 의한 일반 수평 이동 속도를 만들어 Mover에게 전달한다.
    public class WalkVelocitySource : IVelocitySource
    {
        private readonly IPlayerStateEventSubscriber _subscriber;
        private readonly IStateTriggerRaiser _triggerRaiser;
        private readonly float _moveSpeed;
        private readonly IDisposable _moveEventDisposable;
        private bool _canMove;

        public WalkVelocitySource(IPlayerStateEventSubscriber subscriber, IStateTriggerRaiser triggerRaiser, float moveSpeed)
        {
            _subscriber = subscriber;
            _triggerRaiser = triggerRaiser;
            _moveSpeed = moveSpeed;

            if (_subscriber != null)
            {
                _moveEventDisposable = _subscriber.Register(StateEventCategory.MoveControl, HandleMoveOn, HandleMoveClose);
            }
            else
            {
                // 이동을 제어할 상태 시스템이 없으면 제약 없이 항상 허용한다.
                _canMove = true;
            }
        }

        public Vector3 Evaluate(in MoveParams frame)
        {
            if (!_canMove || frame.MoveDirection == Vector3.zero)
            {
                return Vector3.zero;
            }
            // 실제 이동이 일어났으니 Locomotion 전환을 요청한다(이미 Locomotion이면 머신이 무시).
            _triggerRaiser?.RaiseTrigger(StateTriggerType.Move);
            return frame.MoveDirection * _moveSpeed;
        }

        // 일반 이동은 캐릭터가 바뀌어도 이월되는 누적 상태가 없어서 비움
        public void OnCharacterChanged() { }

        public void Dispose()
        {
            _moveEventDisposable?.Dispose();
        }

        private void HandleMoveOn(IStartData data) => _canMove = true;
        private void HandleMoveClose(CloseEventType reason) => _canMove = false;
    }
}
