using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 슬롯에 적힌 값을 실제 게임에 되돌린다. 자리 옮기기, 체력 맞추기, 캐릭터 맞추기 셋만 한다.
    // 파일도 슬롯도 안 만진다. 그건 SaveSlotManager와 SlotLoadRunner가 한다.
    // 씬마다 하나씩 있어야 한다. 주입이 씬이 뜰 때 한 번뿐이라 씬을 넘어 살아남으면 안 된다.
    public class GameStateSaver : MonoBehaviour
    {
        [Inject(true)] private ICurrentCharacterProvider _character;
        [Inject(true)] private ICharacterSwappable _swapper;
        [Inject(true)] private IHealthModifier _health;
        [Inject(true)] private PlayerPartner _partner;

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
