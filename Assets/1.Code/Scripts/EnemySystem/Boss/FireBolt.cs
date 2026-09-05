using UnityEngine;
    
namespace Refactoring
{
    public class FireBolt : MonoBehaviour
    {
        ICurrentCharacterProvider currentCharacterProvider;
        public float damage;
        public float speed;
        public GameObject impactEffect;


        public void Init(ICurrentCharacterProvider provider)
        {
            currentCharacterProvider = provider;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (currentCharacterProvider == null)
            {
                return;
            }

            if (other.transform == currentCharacterProvider.GetCurrentComponent<Transform>())
            {
                PlayerStateMachine playerStateMachine = currentCharacterProvider.GetCurrentComponent<PlayerStateMachine>();
                PlayerDamageReceiver playerDamageReceiver = currentCharacterProvider.GetCurrentComponent<PlayerDamageReceiver>();

                if(playerStateMachine.CurrentState.StateKey != PlayerStateType.Dead)
                {
                    var message = new DamageInfo();
                    message.Amount = damage;
                    message.Damager = gameObject;
                    playerDamageReceiver.ApplyDamage(message);
                }

                if (impactEffect != null)
                {
                    Instantiate(impactEffect, transform.position, Quaternion.identity);
                }
            }
        }
    }
}