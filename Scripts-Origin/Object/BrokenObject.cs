using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenObject : Enemy
{
    public ParticleSystem hpEffect;
    public ParticleSystem DieEffect;

    private float restoreHP;
    private float restoreHPCount;

    private bool isInvincible = false;


    [Header("죽일 적 넣는 곳")]
    public List<Enemy> enemies = new List<Enemy>();

    protected override void Awake()
    {

    }

    protected override void Start()

    {
        restoreHP = hp;
        restoreHPCount = hpBarCount;

        GetHP();

        enemyState = eState.Idle;
    }
    protected override void Update()
    {

    }

    protected override void FixedUpdate()
    {

    }

    public override void IdleLogic()
    {

    }

    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || damageMessage.damager == gameObject || isDead || isInvincible)
        {
            return false;
        }

        phaseHPs[phaseHPs.Count - 1] -= damageMessage.amount;
        lastDamagedTime = Time.time;
        //Debug.Log($"오브젝트 체력 : {phaseHPs[phaseHPs.Count - 1]}");
        //audioSourceHit.PlaySoundEffect(0);

        if (phaseHPs[phaseHPs.Count - 1] <= 0)
        {
            ClearHPBar();
        }

        return true;
    }

    public override void DieAction()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && !enemy.isDead)
            {
                enemy.DieAction();
            }
        }
        DieEffect.Play();
        this.gameObject.tag = "Untagged";
        this.gameObject.layer = 0;

        if (KillTrigger != null)
        {
            KillTrigger.OnEnemyKilled(this);
        }

        if (isKillTrigger)
        {
            while (delayTimes.Count < DeathEvent.Count)
            {
                delayTimes.Add(0f);
            }
            for (int i = 0; i < DeathEvent.Count; i++)
            {
                if (DeathEvent[i] != null)
                {
                    StartCoroutine(ExecuteEventWithDelay(DeathEvent[i], delayTimes[i]));
                }
            }
        }

    }
    private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventData.Execute();
    }

    public void ResetController()
    {
        GetHP();
        GetComponent<Collider>().enabled = true;
    }

    public void GetHP()
    {
        hp = restoreHP;
        hpBarCount = restoreHPCount;
        DieEffect.Stop();
        hpEffect.Play();
        phaseHPs = new List<float>();
        phaseHP = hp / hpBarCount;
        for (var i = 0; i < hpBarCount; i++)
        {
            phaseHPs.Add(phaseHP);
        }
    }
}
