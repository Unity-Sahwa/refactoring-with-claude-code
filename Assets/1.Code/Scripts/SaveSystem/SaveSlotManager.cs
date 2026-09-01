using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 세이브 슬롯 n개를 맡는다. 어느 칸에 쓸지 고르고, 목록을 최신순으로 주고, 전부 지운다.
    // 파일과 칸만 안다. 씬을 띄우고 값을 되돌리는 일은 SlotLoadRunner가 한다.
    public class SaveSlotManager : MonoBehaviour
    {
        // 저장 시각을 적는 모양. 앞자리가 큰 단위라 글자끼리 비교해도 시간순이 된다.
        private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        [SerializeField]
        private int _slotCount = 4;

        [Preserve, Inject(true)] private ISaveService _saveService;

        // 지금 놀고 있는 칸. 아직 정해지지 않았으면 -1.
        //
        // static인 이유:
        // 씬이 바뀌면 씬 안의 오브젝트는 다 파괴돼서, 새 씬의 SaveSlotManager는 값이 초기값으로 돌아간다.
        // 그런데 static 필드는 오브젝트가 아니라 클래스에 붙어 있어서 씬이 바뀌어도 그대로 남는다.
        //
        // DontDestroyOnLoad로 오브젝트째 살리지 않는 이유:
        // AttributeInjector가 씬이 뜰 때마다 씬 안의 오브젝트를 전부 모아서 주입 대상으로 등록한다.
        // 오브젝트를 살려두면 살아남은 옛것과 새 씬에 놓인 사본이 둘 다 등록돼서,
        // "구현이 2개, 첫 번째만 주입" 경고가 뜨고 엉뚱한 쪽이 주입될 수 있다.
        // 씬을 넘어 남아야 하는 건 오브젝트가 아니라 이 값 하나뿐이라, 값만 남기면 그 충돌이 아예 안 생긴다.
        private static int _currentSlot = -1;

        public int CurrentSlot => _currentSlot;

        // static은 게임을 껐다 켜야 지워진다. 에디터에서 도메인 리로드를 끄면 지난 판의 값이 그대로 딸려 온다.
        // 그래서 게임이 시작될 때 손으로 비운다. AttributeInjector.CollectType도 같은 이유로 같은 짓을 한다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void ResetStatic()
        {
            _currentSlot = -1;
        }

        // 저장 시각 최신순. 빈 칸은 뒤로 민다.
        public SaveSlotInfo[] GetSlots()
        {
            SaveSlotInfo[] slots = new SaveSlotInfo[_slotCount];

            for (int i = 0; i < _slotCount; i++)
            {
                GameSaveData data = Load(i);

                slots[i] = new SaveSlotInfo
                {
                    Index = i,
                    SavePointKey = data == null ? string.Empty : data.SavePointKey,
                    SavedTime = data == null ? string.Empty : data.SavedTime,
                    IsEmpty = data == null,
                };
            }

            Array.Sort(slots, CompareByTimeDesc);
            return slots;
        }

        // 세이브 포인트를 지날 때 부른다. 저장한 칸이 현재 칸이 된다.
        public void Save(GameSaveData data)
        {
            data.SavedTime = DateTime.Now.ToString(TimeFormat);

            int slot = PickSlot(data.SavePointKey);
            _saveService?.Save(data, slot);
            _currentSlot = slot;
        }

        // 그 칸의 내용을 읽어 주고, 그 칸을 현재 칸으로 삼는다. 되돌리는 일은 부른 쪽이 한다.
        public GameSaveData TakeSlot(int index)
        {
            GameSaveData data = Load(index);

            if (data == null)
            {
                return null;
            }

            _currentSlot = index;
            return data;
        }

        // 지금 놀고 있는 칸의 내용. 낙하 때 체력만 고쳐 다시 쓰려고 꺼낸다.
        public GameSaveData GetCurrentData()
        {
            return _currentSlot < 0 ? null : Load(_currentSlot);
        }

        // 칸을 새로 고르지 않고 지금 칸에 그대로 덮어쓴다. 저장 시각도 그대로 둔다.
        public void OverwriteCurrent(GameSaveData data)
        {
            if (_currentSlot < 0)
            {
                return;
            }

            _saveService?.Save(data, _currentSlot);
        }

        // 새 게임을 시작할 때 부른다.
        public void DeleteAll()
        {
            for (int i = 0; i < _slotCount; i++)
            {
                _saveService?.Delete<GameSaveData>(i);
            }

            _currentSlot = -1;
        }

        // 쓸 칸 고르기: 같은 지점 → 빈 칸 → 가장 오래된 칸 순.
        // 같은 지점을 덮어쓰는 이유는, 같은 자리 기록이 여러 칸을 차지하면 되돌아갈 곳이 안 늘어나기 때문이다.
        private int PickSlot(string savePointKey)
        {
            int emptySlot = -1;
            int oldestSlot = 0;
            string oldestTime = null;

            // 중간에 안 끊고 끝까지 본다. 빈 칸이 앞에 있어도 뒤에 같은 지점이 있으면 그쪽이 우선이다.
            for (int i = 0; i < _slotCount; i++)
            {
                GameSaveData data = Load(i);

                if (data == null)
                {
                    if (emptySlot < 0)
                    {
                        emptySlot = i;
                    }

                    continue;
                }

                if (data.SavePointKey == savePointKey)
                {
                    return i;
                }

                if (oldestTime == null || string.CompareOrdinal(data.SavedTime, oldestTime) < 0)
                {
                    oldestTime = data.SavedTime;
                    oldestSlot = i;
                }
            }

            return emptySlot >= 0 ? emptySlot : oldestSlot;
        }

        private GameSaveData Load(int slot)
        {
            return _saveService?.Load<GameSaveData>(slot);
        }

        // 최신이 앞. 빈 칸은 시각이 빈 글자라 자연히 뒤로 간다.
        private int CompareByTimeDesc(SaveSlotInfo a, SaveSlotInfo b)
        {
            return string.CompareOrdinal(b.SavedTime, a.SavedTime);
        }
    }
}
