using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static WisuMainRe;

public class FirePillar : MonoBehaviour
{
    public enum Mode
    {
        Standing,
        Tracking
    }

    [Header("모드")]
    public Mode pillarMode;

    [Header("데미지")]
    public float damage = 1;

    [Header("파괴 되는 시간")]
    public float lifeTime = 10;

    [Header("속도")]
    public float speed = 10;

    [HideInInspector]
    public Vector3 direction;

    private Player player;
    private Rigidbody rigidbody;
    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        player = GameObject.FindWithTag("PlayerScript").GetComponent<Player>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (pillarMode == Mode.Standing)
        {
            speed = 0;
            // 네비메쉬 비활성화
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
            }

        }
        else if (pillarMode == Mode.Tracking)
        {
            // 네비메쉬 활성화
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = true;
                navMeshAgent.speed = speed;
                navMeshAgent.SetDestination(player.transform.position);
            }
        }
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (pillarMode == Mode.Tracking && navMeshAgent != null)
        {
            // 플레이어 추적
            navMeshAgent.SetDestination(player.transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && player != null && !player.CheckDie())
        {
            var message = new DamageMessage
            {
                amount = damage
            };
            player.ApplyDamage(message);
        }
    }
}
