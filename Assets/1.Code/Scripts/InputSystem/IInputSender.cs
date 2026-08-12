using UnityEngine;

namespace Refactoring
{
    // 키보드/패드 말고 다른 곳(모바일 UI 버튼 등)에서 입력을 밀어넣을 때 쓰는 창구.
    // InputHub가 구현함. 여기로 들어온 입력은 키를 눌렀을 때와 똑같은 길로 흐름.
    public interface IInputSender
    {
        void RoutePressed(InputActionType action);
        void RouteMove(Vector2 move);
    }
}
