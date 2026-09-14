namespace Refactoring
{
    // 책임: 강조 표시(아웃라인 등)를 켜고 끄는 대상임을 알려준다.
    public interface IHighlightable
    {
        void SetOutline(bool isOn);
    }
}
