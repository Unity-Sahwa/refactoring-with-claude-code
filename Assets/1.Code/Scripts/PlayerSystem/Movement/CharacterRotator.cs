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
        private readonly ILockOnTarget _lockOnTarget;
        private readonly IDisposable _rotateEventDisposal;
        private bool _canRotate;


        public CharacterRotator(IPlayerStateEventSubscriber subscriber, float rotateRate, ILockOnTarget lockOnTarget)
        {
            _subscriber = subscriber;
            _rotateRate = rotateRate;
            _lockOnTarget = lockOnTarget;


            if (_subscriber != null)
            {
                _rotateEventDisposal = _subscriber.Register(StateEventCategory.RotateControl, HandleRotateOn, HandleRotateClose);
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

            Vector3 lookDir;
            Collider locked = _lockOnTarget?.LockedTarget;
            if (locked != null)
            {
                // 락온: 이동 중이어도 고정된 적을 바라봄(스트레이프·백스텝).
                lookDir = locked.transform.position - frame.CharacterTransform.position;
                lookDir.y = 0f;
                if (lookDir == Vector3.zero) return;   // 적과 겹칠 때 방어
            }
            else
            {
                // 비락온: 이동 방향을 바라봄.
                lookDir = frame.MoveDirection;
            }

            Quaternion target = Quaternion.LookRotation(lookDir);
            frame.CharacterTransform.rotation =
                Quaternion.Slerp(frame.CharacterTransform.rotation, target, frame.DeltaTime * _rotateRate);
        }

        public void Dispose()
        {
            _rotateEventDisposal?.Dispose();
        }

        private void HandleRotateOn(IStartData data) => _canRotate = true;
        // End(구간 끝)든 Reset(강제 이탈)이든 회전 허용을 끄는 동작은 같아서 이유는 보지 않는다.
        private void HandleRotateClose(CloseEventType reason) => _canRotate = false;
    }
}
