using UnityEngine;

public class WaterBall : MonoBehaviour
{
    public EnemyDataRange enemyDataRange;

    public float damage;
    public float lifeTime;
    public float homingSpeed = 2f; // 유도 속도
    public float homingDuration = 2f; // 유도 지속 시간
    private Player player;
    private Rigidbody rigidbody;
    private bool isHomingActive = true; // 유도 활성화 상태
    private float homingTimer = 0f; // 유도 시간을 관리할 타이머

    private void Start()
    {
        damage = enemyDataRange.f_autoAttackRangedDamage;
        player = GameObject.FindWithTag("PlayerScript").GetComponent<Player>();
        lifeTime = enemyDataRange.f_deleteTime;
        Destroy(gameObject, lifeTime); // 일정 시간 후 삭제

        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Time.deltaTime을 사용하여 homingDuration이 지나면 유도 기능을 비활성화
        if (isHomingActive)
        {
            homingTimer += Time.deltaTime;
            if (homingTimer >= homingDuration)
            {
                isHomingActive = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isHomingActive && player != null && !player.CheckDie())
        {
            // 플레이어와 동일한 y축을 유지하고, xz축 방향으로 유도
            Vector3 targetPosition = player.transform.position;
            targetPosition.y = transform.position.y; // 발사체의 y축 고정

            // XZ축으로 유도
            Vector3 directionToPlayer = (targetPosition - transform.position).normalized;
            Vector3 newDirection = Vector3.Lerp(rigidbody.velocity.normalized, directionToPlayer, homingSpeed * Time.fixedDeltaTime).normalized;

            rigidbody.velocity = newDirection * rigidbody.velocity.magnitude; // 속력 유지한 채로 방향 수정
        }
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
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Default") || other.gameObject.layer != LayerMask.NameToLayer("Enemy"))
        {
            Destroy(gameObject);
        }
    }

    //public EnemyDataRange enemyDataRange;

    //public float damage;
    //public float lifeTime;
    //private Player player;


    //private void Start()
    //{
    //    damage = enemyDataRange.f_autoAttackRangedDamage;
    //    player = GameObject.FindWithTag("Player").GetComponent<Player>();
    //    lifeTime = enemyDataRange.f_deleteTime;
    //    Destroy(gameObject, lifeTime);
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        if (player != null && !player.CheckDie())
    //        {
    //            var message = new DamageMessage();
    //            message.amount = damage;
    //            player.ApplyDamage(message);
    //        }
    //        Destroy(gameObject);
    //    }
    //    else if (other.tag == "Untagged" || other.tag != "Enemy") // Untagged와 Enemy가 아닌 모든 태그에 적용
    //    {
    //        Destroy(gameObject);
    //    }
    //}
}
