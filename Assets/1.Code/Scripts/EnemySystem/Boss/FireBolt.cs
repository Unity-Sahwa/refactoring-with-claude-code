using UnityEngine;
    
namespace Refactoring
{
    public class FireBolt : MonoBehaviour
    {
        ICurrentCharacterProvider currentCharacterProvider;
        public float damage;
        public float lifeTime = 10;
        public float speed;
        [HideInInspector] public float destroyTime;
        public Rigidbody rb;
        public GameObject impactEffect;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();

            if (destroyTime > 0)
            {
                lifeTime = destroyTime;
            }

            Destroy(gameObject, lifeTime);
        }

        public void Init(ICurrentCharacterProvider provider)
        {
            currentCharacterProvider = provider;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == currentCharacterProvider.CurrentCharacter.gameObject)
            {
                PlayerStateMachine playerStateMachine = currentCharacterProvider.CurrentCharacter.GetComponent<PlayerStateMachine>();
                PlayerDamageReceiver playerDamageReceiver = currentCharacterProvider.CurrentCharacter.GetComponent<PlayerDamageReceiver>();

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