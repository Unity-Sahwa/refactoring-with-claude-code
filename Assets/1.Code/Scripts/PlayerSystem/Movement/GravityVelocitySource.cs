using UnityEngine;

namespace Refactoring
{
    // 무엇: 중력을 담당. 급경사일 경우 중력으로 플레이어가 미끄러지게 만듬
    // 왜 직접 미끄러뜨리나: CharacterController는 Slope Limit 초과 경사에서 "올라가기"만 막을 뿐 미끄러뜨리진 않는다.
    //                       그래서 발밑 면이 한계각보다 가파르면 쌓인 중력 속도를 그 면을 따라 흐르게 투영해 직접 미끄러뜨린다.
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
                //대원_Study: planeNormal이 법선인 평면에 vector를 내린 것
                return Vector3.ProjectOnPlane(new Vector3(0f, _verticalSpeed, 0f), frame.GroundNormal);
            }
            return new Vector3(0f, _verticalSpeed, 0f);
        }

        // 캐릭터가 바뀌면 누적된 낙하속도를 초기화(이전 캐릭터의 낙하가 새 캐릭터로 이월되지 않게).
        public void OnCharacterChanged() => _verticalSpeed = 0f;

        public void Dispose() { }
    }
}
