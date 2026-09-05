namespace Refactoring
{
    // 책임: 포인터(마우스·터치)로 카메라를 돌릴 수 있는지 켜고 끈다. (모바일은 화면 문지르기가 이동 조작이라 꺼야 한다)
    public interface IPointerLookControl
    {
        void SetPointerLookEnabled(bool enabled);
    }
}
