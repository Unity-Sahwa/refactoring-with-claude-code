using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Refactoring
{
    // 세이브 포인트. 플레이어가 닿으면(EnterTrigger가 Execute를 부름) 지금 상태를 슬롯에 저장한다.
    // 씬 시작용으로 체크해두면 시작하자마자 저장한다. 그 자리가 곧 부활 지점이 된다.
    public class EventSaveGame : EventData
    {
        // 이 지점의 이름표이자 지역 이름을 꺼낼 번역 표의 키. 같은 지점이면 새 칸을 쓰지 않고 그 칸에 덮어쓴다.
        // 인스펙터에서 표의 키를 드롭다운으로 고른다. Area가 든 키만 보인다.
        [TextKey("Area")] [SerializeField] private string _savePointKey;

        // 이 지점에서 같이 기억할 오브젝트들. Active를 체크하면 불러올 때 켜진 채로, 끄면 꺼진 채로 되돌린다.
        [SerializeField] private List<ObjectActiveEntry> _objectStates;

        // 체크하면 플레이어를 기다리지 않고 씬이 시작할 때 바로 저장한다.

        [Inject(true)] private IHealthInfo _health;
        [Inject(true)] private SaveSlotManager _slots;
        [Inject(true)] private ICurrentCharacterProvider _character;


        public override void Execute()
        {
            if (_slots == null)
            {
                return;
            }

            GameSaveData data = new GameSaveData
            {
                SavePointKey = _savePointKey,
                SceneIndex = SceneManager.GetActiveScene().buildIndex,

                // 플레이어 좌표가 아니라 이 지점의 자리를 쓴다. 벽에 낀 자리에서 저장되는 사고가 없다.
                PlayerPosition = transform.position,
                Hp = _health != null ? _health.Current : 0f,

                // 주입이 안 됐으면 사람 캐릭터로 본다. 게임 시작이 사람 캐릭터라서 그게 덜 어긋난다.
                CharacterType = _character != null ? _character.CurrentCharacter.Type : PlayerCharacterType.HumanCharacter,
                ObjectStates = BuildObjectStates(),
            };

            _slots.Save(data);
        }

        private List<ObjectActiveState> BuildObjectStates()
        {
            if (_objectStates == null || _objectStates.Count == 0)
            {
                return null;
            }

            List<ObjectActiveState> result = new List<ObjectActiveState>(_objectStates.Count);

            foreach (ObjectActiveEntry entry in _objectStates)
            {
                if (entry.Target == null)
                {
                    continue;
                }

                result.Add(new ObjectActiveState
                {
                    Path = ObjectActiveState.GetPath(entry.Target.transform),
                    Active = entry.Active,
                });
            }

            return result;
        }
    }
}
