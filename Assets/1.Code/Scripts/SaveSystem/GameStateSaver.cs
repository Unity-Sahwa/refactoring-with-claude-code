using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 슬롯에 적힌 값을 실제 게임에 되돌린다. 자리 옮기기, 체력 맞추기, 캐릭터 맞추기 셋만 한다.
    // 파일도 슬롯도 안 만진다. 그건 SaveSlotManager와 SlotLoadRunner가 한다.
    // 씬마다 하나씩 있어야 한다. 주입이 씬이 뜰 때 한 번뿐이라 씬을 넘어 살아남으면 안 된다.
    public class GameStateSaver : MonoBehaviour
    {
        [Preserve, Inject(true)] private ICurrentCharacterProvider _character;
        [Preserve, Inject(true)] private ICharacterSwappable _swapper;
        [Preserve, Inject(true)] private IHealthModifier _health;
        [Preserve, Inject(true)] private PlayerPartner _partner;

        // 씬을 그냥 넘어온 경우, EventSceneLoad가 넘겨둔 체력과 캐릭터만 이어받는다. 자리는 새 씬 것을 그대로 쓴다.
        //
        // Start인 이유: 씬이 뜨는 차례는 Awake/OnEnable → sceneLoaded → Start다.
        // 불러오기 복원(SlotLoadRunner)이 sceneLoaded에서 돌기 때문에, Start 시점엔 LoadedFromSlot이 이미 정해져 있다.
        private void Start()
        {
            if (!EventSceneLoad.TakePending(out float hp, out PlayerCharacterType type))
            {
                return;
            }

            // 불러오기로 뜬 씬이면 슬롯에 적힌 값이 맞다. 넘어온 값으로 덮으면 안 된다.
            if (SlotLoadRunner.LoadedFromSlot)
            {
                return;
            }

            RestoreCharacter(type);
            _health?.SetCurrent(hp);
        }

        public void Restore(GameSaveData data)
        {
            if (data == null)
            {
                return;
            }

            // 캐릭터를 먼저 맞춘다. 스왑이 두 캐릭터의 자리를 서로 옮기기 때문에,
            // 자리를 먼저 넣으면 스왑하면서 그 자리가 덮인다.
            RestoreCharacter(data.CharacterType);
            RestorePosition(data.PlayerPosition);

            _health?.SetCurrent(data.Hp);

            RestoreObjectStates(data.ObjectStates);
        }

        private void RestoreObjectStates(List<ObjectActiveState> states)
        {
            if (states == null)
            {
                return;
            }

            foreach (ObjectActiveState state in states)
            {
                Transform target = ObjectActiveState.Find(state.Path);
                target?.gameObject.SetActive(state.Active);
            }
        }

        private void RestoreCharacter(PlayerCharacterType type)
        {
            if (_character?.CurrentCharacter == null || _swapper == null)
            {
                return;
            }

            if (_character.CurrentCharacter.Type != type)
            {
                // 되돌리는 스왑은 플레이어가 한 게 아니라서 스왑 소리를 내지 않는다.
                if (_partner != null)
                {
                    _partner.SkipNextCollideSound = true;
                }

                _swapper.SwapPlayerCharacter();
            }
        }

        private void RestorePosition(SerializableVector3 position)
        {
            PlayerCharacter current = _character?.CurrentCharacter;

            if (current == null)
            {
                return;
            }

            CharacterController controller = current.GetCharacterComponent<CharacterController>();

            // CharacterController가 켜져 있으면 자기가 자리를 계속 붙잡아서 position을 넣어도 도로 돌아온다.
            if (controller != null)
            {
                controller.enabled = false;
                current.transform.position = position;
                controller.enabled = true;
                return;
            }

            current.transform.position = position;
        }
    }
}
