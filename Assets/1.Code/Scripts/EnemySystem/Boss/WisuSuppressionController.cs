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

    private float restoreHP;
    private float restoreHPCount;

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

        if (phaseHPs[phaseHPs.Count - 1] <= 0)
        {
            ClearHPBar();
        }
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