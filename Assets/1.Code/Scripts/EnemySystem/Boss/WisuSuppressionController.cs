using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
public class WisuSuppressionController : Enemy
{
    [Inject] private WisuMainRe wisuMain;
    [Inject] private WisuSpawnPhase wisuSpawnPhase;
    public EnemyDataMelee ControllerData;
    public bool isInvincible = false;
    public ParticleSystem hpEffect;
    public ParticleSystem DieEffect;

    [Header("피격 시 흔들릴 오브젝트")]
    public Transform shakeTarget;
    public float shakeDuration = 0.2f;
    public float shakeStrength = 0.1f;

    private float restoreHP;
    private float restoreHPCount;
    private Coroutine shakeCoroutine;

    public List<EventData> deactivateEvents;

    public List<EventData> activateEvents;


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

    public override void ApplyDamage(DamageInfo damageMessage)
    {
        if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || damageMessage.Damager == gameObject || isDead || isInvincible)
        {
            return;
        }

        phaseHPs[phaseHPs.Count - 1] -= damageMessage.Amount;
        lastDamagedTime = Time.time;

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
        DieEffect.Play();
        wisuMain.isControllerActive = true;
        wisuSpawnPhase.isControllerActive = true;
        GetComponent<Collider>().enabled = false;

        Invoke("InvokeExecuteIndicator", 3);
    }

    public void InvokeExecuteIndicator()
    {
        for (int i = 0; i < deactivateEvents.Count; i++)
        {
            deactivateEvents[i].Execute();
        }
    }

    public void ResetController()
    {     
        GetHP();
        wisuMain.isControllerActive = false;
        wisuSpawnPhase.isControllerActive = false;
        GetComponent<Collider>().enabled = true;
        
        for (int i = 0; i < activateEvents.Count; i++)
        {
            activateEvents[i].Execute();
        }
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
}