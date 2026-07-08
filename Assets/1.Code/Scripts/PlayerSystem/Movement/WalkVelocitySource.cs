using System;
using UnityEngine;

namespace Refactoring
{
    // 무엇: 사용자 입력에 의한 "일반 수평 이동" 속도를 만들어 Mover에게 전달한다.
    // 왜 존재: 기존 독립적으로 입력을 받아 이동하는 방식에서 CharacterController.Move로 변경되면서 이동을 한곳에 모으고자 이동 소스로서 존재. 
    //          걷는 이동을 계산해서 Mover에게 보내는 역할을 담당하게됨.
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
                _moveEventDisposable = _subscriber.RegisterEventSwitch(StateEventCategory.MoveControl, HandleMoveOn, HandleMoveClose);
            }
            else
            {
                _canMove = true; // 이동을 제어할 상태 시스템이 없으면 제약 없이 항상 허용.
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

        private void HandleMoveOn(PlayerCharacter source, IStartData data) => _canMove = true;
        private void HandleMoveClose(CloseEventType reason) => _canMove = false;
    }
}
