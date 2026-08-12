namespace Refactoring
{
    // 불러오기 창에 한 줄씩 뿌릴 내용. 글자는 세이브 쪽에서 만들어 주고 UI는 그대로 찍기만 한다.
    public struct SaveSlotInfo
    {
        public int Index;
        public string ZoneName;
        public string SavedTime;
        public bool IsEmpty;
    }

    // 세이브 시스템에 부탁하는 창구. UI는 파일이 몇 개인지, 어떤 형식인지 모른다.
    public interface ISaveSlots
    {
        SaveSlotInfo[] GetSlots();
        void LoadSlot(int index);
    }
}
