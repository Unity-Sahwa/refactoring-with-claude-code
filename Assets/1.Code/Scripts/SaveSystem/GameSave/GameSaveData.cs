using System;
using System.Collections.Generic;

namespace Refactoring
{
    // 세이브 슬롯 하나에 들어가는 내용. 세이브 포인트를 지날 때 이 모양으로 파일에 적힌다.
    [Serializable]
    public class GameSaveData : ISaveData
    {
        public const string FileName = "GameSaveData";

        // 저장한 때. 불러오기 창을 최신순으로 줄 세울 때와 화면에 찍을 때 쓴다.
        public string SavedTime;

        // 어느 세이브 포인트에서 저장했는지. 같은 지점이면 새 칸을 쓰지 않고 덮어쓴다.
        // 값이 번역 표의 키라서, 불러오기 창이 이걸 그대로 표에 넣어 지역 이름을 꺼낸다.
        public string SavePointKey;

        public int SceneIndex;

        // 저장한 순간에 움직이던 캐릭터. 되돌릴 때 이 쪽이 아니면 한 번 스왑한다.
        public PlayerCharacterType CharacterType;

        public SerializableVector3 PlayerPosition;
        public float Hp;

        // 세이브 포인트에서 지정한 오브젝트들의 켜짐/꺼짐 상태.
        public List<ObjectActiveState> ObjectStates;
    }
}
