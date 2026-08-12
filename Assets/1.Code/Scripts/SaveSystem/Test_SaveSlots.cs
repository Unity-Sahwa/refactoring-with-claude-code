using UnityEngine;

namespace Refactoring
{
    // 불러오기 창을 눈으로 확인하려고 만든 가짜 슬롯. 진짜 세이브 시스템에 슬롯 개념이 생기면 지운다.
    public class Test_SaveSlots : MonoBehaviour, ISaveSlots
    {
        [SerializeField]
        private int _slotCount = 3;

        public SaveSlotInfo[] GetSlots()
        {
            SaveSlotInfo[] slots = new SaveSlotInfo[_slotCount];

            for (int i = 0; i < _slotCount; i++)
            {
                slots[i] = new SaveSlotInfo
                {
                    Index = i,
                    ZoneName = $"테스트 지역 {i + 1}",
                    SavedTime = "2026-07-29 12:00",
                    // 마지막 칸은 빈 슬롯으로 둬서 비활성 표시도 같이 확인한다.
                    IsEmpty = i == _slotCount - 1,
                };
            }

            return slots;
        }

        public void LoadSlot(int index)
        {
            Debug.Log($"[Test_SaveSlots] {index}번 슬롯 불러오기 요청");
        }
    }
}
