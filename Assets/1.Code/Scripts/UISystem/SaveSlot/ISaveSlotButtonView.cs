using TMPro;

namespace Refactoring
{
    // 책임: 슬롯 버튼 하나의 겉모습을 채울 수 있다는 것만 약속한다.
    public interface ISaveSlotButtonView
    {
        void Fill(string zoneName, string savedTime, bool isEmpty, TMP_FontAsset font);
    }
}
