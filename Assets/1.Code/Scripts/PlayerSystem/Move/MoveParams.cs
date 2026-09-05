using UnityEngine;

namespace Refactoring
{
    // 책임: 한 프레임의 이동 계산에 필요한 읽기용 데이터 묶음. (속도 소스와 회전기에게 전달된다)
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
