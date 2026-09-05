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
        public float speed = 10;
        
        [HideInInspector]
        public Vector3 direction;
        private Rigidbody rb;
        private NavMeshAgent navMeshAgent;

        // 풀에서 빌려줄 때마다 불린다. 초기화를 Start에 두면 재사용 때 다시 돌지 않는다.
        public void Init(ICurrentCharacterProvider provider)
        {
            currentCharacterProvider = provider;
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
                    navMeshAgent.SetDestination(currentCharacterProvider.GetCurrentComponent<Transform>().position);
                }
            }
        }

        private void Update()
        {
            if (pillarMode == Mode.Tracking && navMeshAgent != null && currentCharacterProvider != null)
            {
                navMeshAgent.SetDestination(currentCharacterProvider.GetCurrentComponent<Transform>().position);
            }
        }

        private void OnTriggerStay(Collider other)
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
                };
            }
        }
    }
}