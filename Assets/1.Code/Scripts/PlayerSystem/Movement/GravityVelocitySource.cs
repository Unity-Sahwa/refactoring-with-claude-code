using UnityEngine;

namespace Refactoring
{
    // 책임: 중력 속도를 만들고, 한계각보다 가파른 경사에서는 직접 미끄러뜨린다.
    //       (CharacterController는 한계각 초과 경사에서 "올라가기"만 막을 뿐 미끄러뜨리지 않는다)
    public class GravityVelocitySource : IVelocitySource
    {
        private readonly float _gravity;        // 직접 내려주는 중력 가속도(음수)
        private readonly float _maxFallSpeed;   // 최대 낙하 속도(음수)
        private readonly float _groundedStick;  // 착지 시 바닥에 살짝 눌러 붙이는 속도(음수)

        private float _verticalSpeed;

        public GravityVelocitySource(float gravity, float maxFallSpeed, float groundedStick)
        {
            _gravity = gravity;
            _maxFallSpeed = maxFallSpeed;
            _groundedStick = groundedStick;
        }

        public Vector3 Evaluate(in MoveParams frame)
        {
            // 각도만으로 가파른 경사 판정(isGrounded 제외 → 벽 옆구리에 닿아 isGrounded가 false여도 미끄러짐 유지).
            float groundAngle = Vector3.Angle(frame.GroundNormal, Vector3.up);
            bool onSteepSlope = groundAngle > frame.Controller.slopeLimit;

            // 중력은 항상 아래로 쌓는다.
            _verticalSpeed += _gravity * frame.DeltaTime;
            _verticalSpeed = Mathf.Max(_verticalSpeed, _maxFallSpeed);

            // 걸을 수 있는 바닥에 착지했으면 떨어지는 속도를 멈춰 바닥에 붙인다.
            if (frame.Controller.isGrounded && !onSteepSlope && _verticalSpeed < 0f)
            {
                _verticalSpeed = _groundedStick;
            }

            if (onSteepSlope)
            {
                // 가파른 경사: 쌓인 중력 속도를 경사면을 따라 흐르게 투영 → 면을 타고 미끄러져 내려간다.
                return Vector3.ProjectOnPlane(new Vector3(0f, _verticalSpeed, 0f), frame.GroundNormal);
            }
            return new Vector3(0f, _verticalSpeed, 0f);
        }

        // 캐릭터가 바뀌면 누적된 낙하속도를 초기화(이전 캐릭터의 낙하가 새 캐릭터로 이월되지 않게).
        public void OnCharacterChanged() => _verticalSpeed = 0f;

        public void Dispose() { }
    }
}
