namespace Refactoring
{
    // 설정값 주인들의 공통 창구. 설정창이 닫힐 때 "다들 저장해"라고 한 번에 시키려고 쓴다.
    // 설정창은 이것만 알고, 감도인지 소리인지는 모른다.
    public interface ISettingsHolder
    {
        void Save();
    }
}
