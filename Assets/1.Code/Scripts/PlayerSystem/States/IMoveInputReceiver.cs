using UnityEngine;

namespace Refactoring
{
    // 이동 입력 벡터가 필요한 State용 인터페이스. 머신이 Update 전에 SetMoveInput으로 넣어준다.
    // 베이스 CharacterState는 입력을 모르고, 필요한 State(현재 LocomotionState)만 구현한다.
    public interface IMoveInputReceiver
    {
        void SetMoveInput(Vector2 moveInput);
    }
}
