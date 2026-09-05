using UnityEngine;

namespace Refactoring
{
    // 책임: 지금 이동 애니메이션이 표현 중인 방향을 값으로 알려준다.
    // 왜? : 다른 시스템이 Animator 파라미터 이름을 문자열로 훔쳐보지 않게 하기 위함.
    public interface IMoveDirectionProvider
    {
        // 이동 중이 아니면 Vector2.zero.
        Vector2 MoveDirection { get; }
    }
}
