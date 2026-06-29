using UnityEngine;

namespace Refactoring
{
    // 역할: 한 프레임에서 이동 계산에 필요한 읽기용 데이터 묶음
    // 왜 존재? : 속도 소스(IVelocitySource)와 회전기(CharacterRotator)에게 전달하여 참고하도록 하기 위함
    public readonly struct MoveParams
    {
        public readonly float DeltaTime;
        public readonly Transform CharacterTransform;
        public readonly CharacterController Controller;
        public readonly Vector3 GroundNormal;
        public readonly Vector3 MoveDirection;

        public MoveParams(float deltaTime, Transform characterTransform, CharacterController controller,
                         Vector3 groundNormal, Vector3 moveDirection)
        {
            DeltaTime = deltaTime;
            CharacterTransform = characterTransform;
            Controller = controller;
            GroundNormal = groundNormal;
            MoveDirection = moveDirection;
        }
    }
}
