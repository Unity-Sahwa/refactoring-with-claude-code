using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 플레이어를 정해둔 자리로 옮기고 방향을 맞춘다.
    public class EventPlayerWarp : EventData
    {
        [SerializeField] private Transform _playerTransform;
        [Preserve, Inject(true)] private ICurrentCharacterProvider _character;

        private void Awake()
        {
            _playerTransform.gameObject.SetActive(false);
        }

        public override void Execute()
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
            }


            current.transform.position = _playerTransform.position;
            current.transform.rotation = Quaternion.Euler(0f, _playerTransform.eulerAngles.y, 0f);

            if (controller != null)
            {
                controller.enabled = true;
            }
        }
    }
}
