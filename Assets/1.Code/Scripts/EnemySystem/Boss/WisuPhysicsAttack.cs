using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    public class WisuPhysicsAttack : MonoBehaviour
    {
        
        [Preserve, Inject] ICurrentCharacterProvider currentCharacterProvider;
        public float damage;
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform == currentCharacterProvider.GetCurrentComponent<Transform>())
            {
                PlayerStateMachine playerStateMachine = currentCharacterProvider.GetCurrentComponent<PlayerStateMachine>();
                PlayerDamageReceiver playerDamageReceiver = currentCharacterProvider.GetCurrentComponent<PlayerDamageReceiver>();

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