using Refactoring;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenObject : Enemy
{
    public ParticleSystem hpEffect;
    public ParticleSystem DieEffect;

    [Header("피격 시 흔들릴 오브젝트")]
    public Transform shakeTarget;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.1f;

    private float restoreHP;
    private float restoreHPCount;

    private bool isInvincible = false;
    private Coroutine shakeCoroutine;


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

        if (shakeTarget != null)
        {
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(Shake());
        }

        if (phaseHPs[phaseHPs.Count - 1] <= 0)
        {
            ClearHPBar();
        }
    }

    // ponytail: 랜덤 오프셋 흔들림. DOTween 없이 코루틴으로만 처리. 감쇠 곡선 필요하면 그때 추가.
    private IEnumerator Shake()
    {
        Vector3 originalPos = shakeTarget.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float strength = shakeStrength * (1f - elapsed / shakeDuration);
            shakeTarget.localPosition = originalPos + Random.insideUnitSphere * strength;
            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeTarget.localPosition = originalPos;
        shakeCoroutine = null;
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