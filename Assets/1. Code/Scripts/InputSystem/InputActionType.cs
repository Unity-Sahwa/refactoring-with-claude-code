// 게임 내 모든 입력 액션을 정의하는 enum.
// Move는 Vector2 액션 — .inputactions 에셋에서 2D Vector composite으로 정의.
// 새 액션 추가 시 이 enum + .inputactions 에셋에 각각 하나씩 추가.
namespace Refactoring
{
    public enum InputActionType
    {
        Movement,
        NormalAttack,
        SpecialAttack,
        FinishAttack,
        Dash,
        LockOn,
        Menu
    }
}
