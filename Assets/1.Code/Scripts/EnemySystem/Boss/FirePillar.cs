using UnityEngine;
using UnityEngine.AI;

namespace Refactoring
{
    public class FirePillar : MonoBehaviour
    {
        public enum Mode
        {
            Standing,
            Tracking
        }
        ICurrentCharacterProvider currentCharacterProvider;
        public Mode pillarMode;
        public float damage = 1;
        public float lifeTime = 10;
        public float speed = 10;
        
        [HideInInspector]
        public Vector3 direction;
        private Rigidbody rb;
        private NavMeshAgent navMeshAgent;

        private void Start()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();

            if (pillarMode == Mode.Standing)
            {
                speed = 0;
                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = false;
                }

            }
            else if (pillarMode == Mode.Tracking)
            {
                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = true;
                    navMeshAgent.speed = speed;
                    navMeshAgent.SetDestination(currentCharacterProvider.CurrentCharacter.transform.position);
                }
            }
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            if (pillarMode == Mode.Tracking && navMeshAgent != null)
            {
                navMeshAgent.SetDestination(currentCharacterProvider.CurrentCharacter.transform.position);
            }
        }

        public void Init(ICurrentCharacterProvider provider)
        {
            currentCharacterProvider = provider;
        }

        private void OnTriggerStay(Collider other)
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
                };
            }
        }
    }
}