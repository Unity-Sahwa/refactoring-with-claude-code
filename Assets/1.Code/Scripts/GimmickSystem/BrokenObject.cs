using Refactoring;
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


    [Header("���� �� �ִ� ��")]
    public List<Enemy> enemies = new List<Enemy>();

    protected override void Awake() { }
    protected override void Start()
    {
        restoreHP = hp;
        restoreHPCount = hpBarCount;

        GetHP();

        enemyState = eState.Idle;
    }
    protected override void Update() { }
    protected override void FixedUpdate() { }
    public override void IdleLogic() { }

    public override void ApplyDamage(DamageInfo damageMessage)
    {
        if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || damageMessage.Damager == gameObject || isDead || isInvincible)
        {
            return;
        }

        phaseHPs[phaseHPs.Count - 1] -= damageMessage.Amount;
        lastDamagedTime = Time.time;
        //Debug.Log($"������Ʈ ü�� : {phaseHPs[phaseHPs.Count - 1]}");
        //audioSourceHit.PlaySoundEffect(0);

        if (phaseHPs[phaseHPs.Count - 1] <= 0)
        {
            ClearHPBar();
        }
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
        if (DieEffect != null) DieEffect.Play();
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
        if (DieEffect != null) DieEffect.Stop();
        if (hpEffect != null) hpEffect.Play();
        phaseHPs = new List<float>();
        phaseHP = hp / hpBarCount;
        for (var i = 0; i < hpBarCount; i++)
        {
            phaseHPs.Add(phaseHP);
        }
    }
}