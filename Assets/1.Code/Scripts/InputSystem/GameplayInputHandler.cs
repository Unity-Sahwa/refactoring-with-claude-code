using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 게임플레이 모드에서 받은 사용자 입력을 필요한 구독자들에게 그대로 뿌린다(배포만 한다).
    // 허브가 현재 모드의 처리기에게만 입력을 주므로, 이 처리기는 게임플레이 컨텍스트 전용이다.
    // 입력 해석·상태머신 전달·block/buffer는 구독자(PlayerStateInputGate·LockOnCamera 등)가 맡는다.
    // 흐름: 허브가 OnPressed/OnMove 호출 -> OnInputPressed/OnVector2Input로 구독자에게 그대로 발행
    public class GameplayInputHandler : MonoBehaviour, IDomainInputHandler, IInputMoveProvider, IInputPressedProvider
    {
        public event Action<InputActionType> OnInputPressed;
        public event Action<Vector2> OnVector2Input;

        public GameStateType Context => GameStateType.GamePlay;  // 게임플레이 모드일 때만 허브에서 입력이 들어옴

        public void OnPressed(InputActionType action) => OnInputPressed?.Invoke(action);
        public void OnMove(Vector2 move) => OnVector2Input?.Invoke(move);
    }
}
