namespace Refactoring
{
    // 불러오기 창에 한 줄씩 뿌릴 내용. 화면에 찍을 것만 담는다(진짜 저장값은 GameSaveData가 들고 있다).
    public struct SaveSlotInfo
    {
        public int Index;
        public string SavePointKey;
        public string SavedTime;
        public bool IsEmpty;
    }

    // 세이브 시스템에 부탁하는 창구. UI는 파일이 몇 개인지, 어떤 형식인지 모른다.
    public interface ISaveSlots
    {
        // 저장 시각 최신순으로 준다. UI는 받은 순서대로 찍기만 한다.
        SaveSlotInfo[] GetSlots();

        // 그 칸으로 게임을 시작한다. 되돌리는 일은 안에서 다른 놈에게 시킨다.
        void LoadSlot(int index);
    }
}
