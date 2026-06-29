using System;
using UnityEngine;

namespace Refactoring
{
    // 무엇: 입력 방향으로 캐릭터를 회전한다.
    // 왜 이동과 분리했나: 속도와는 별도 책임이고, 회전 기능만 따로 필요한 경우가 있다.
    // 왜 Transform을 회전시키나: CharacterController는 회전 개념이 없어 캐릭터 Transform을 직접 돌린다.
    // 흐름: 상태 이벤트로 회전 허용(RotateControl)을 켜고 끔 → 허용 중이고 입력 방향이 있으면 그 방향을 바라보게 Slerp.
    public class CharacterRotator : IDisposable
    {
        private readonly IPlayerStateEventSubscriber _subscriber;
        private readonly float _rotateRate;
        private bool _canRotate;

        public CharacterRotator(IPlayerStateEventSubscriber subscriber, float rotateRate)
        {
            _subscriber = subscriber;
            _rotateRate = rotateRate;

            if (_subscriber != null)
            {
                _subscriber.Subscribe(StateEventCategory.RotateControl, HandleRotateOn);
                _subscriber.SubscribeEnd(StateEventCategory.RotateControl, HandleRotateOff);
                _subscriber.SubscribeReset(HandleReset);
            }
            else
            {
                _canRotate = true; // 회전을 제어할 상태 시스템이 없으면 제약 없이 항상 허용.
            }
        }

        public void Apply(in MoveParams frame)
        {
            if (!_canRotate || frame.MoveDirection == Vector3.zero)
            {
                return;
            }
            Quaternion target = Quaternion.LookRotation(frame.MoveDirection);
            frame.CharacterTransform.rotation =
                Quaternion.Slerp(frame.CharacterTransform.rotation, target, frame.DeltaTime * _rotateRate);
        }

        public void Dispose()
        {
            if (_subscriber != null)
            {
                _subscriber.Unsubscribe(StateEventCategory.RotateControl, HandleRotateOn);
                _subscriber.UnsubscribeEnd(StateEventCategory.RotateControl, HandleRotateOff);
                _subscriber.UnsubscribeReset(HandleReset);
            }
        }

        private void HandleRotateOn(PlayerCharacter source, IStartData data) => _canRotate = true;
        private void HandleRotateOff() => _canRotate = false;
        // 상태 전환 시 허용이 다음 상태로 새지 않게 끈다.
        private void HandleReset() => _canRotate = false;
    }
}
