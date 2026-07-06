using UnityEngine;

namespace Refactoring
{
    public class WisuPhysicsAttack : MonoBehaviour
    {
        
        [Inject] ICurrentCharacterProvider currentCharacterProvider;
        public float damage;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == currentCharacterProvider.CurrentCharacter)
            {
                PlayerStateMachine playerStateMachine = currentCharacterProvider.CurrentCharacter.GetComponent<PlayerStateMachine>();
                PlayerDamageReceiver playerDamageReceiver = currentCharacterProvider.CurrentCharacter.GetComponent<PlayerDamageReceiver>();

                if(playerStateMachine.CurrentState.StateKey != PlayerStateType.Dead)
                {
                    var message = new DamageInfo();
                    message.Amount = damage;
                    playerDamageReceiver.ApplyDamage(message);
                }
            }
        }
    }
}