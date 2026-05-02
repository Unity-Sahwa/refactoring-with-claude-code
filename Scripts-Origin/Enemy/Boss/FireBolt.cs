using UnityEngine;

public class FireBolt : MonoBehaviour
{
    [Header("데미지")] public float damage;
    [Header("기본 파괴 되는 시간")] public float lifeTime = 10;
    [Header("속도")] public float speed;
    [HideInInspector] public Player player;
    [HideInInspector] public float destroyTime;
    public Rigidbody rigidbody;
    public GameObject impactEffect;

    private void Start()
    {
        player = GameObject.FindWithTag("PlayerScript").GetComponent<Player>();
        rigidbody = GetComponent<Rigidbody>();

        if (destroyTime > 0)
        {
            lifeTime = destroyTime;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (player != null && !player.CheckDie())
            {
                var message = new DamageMessage();
                message.amount = damage;
                player.ApplyDamage(message);
            }

            if (impactEffect != null)
            {
                Instantiate(impactEffect, transform.position, Quaternion.identity);
            }
        }
    }
}
