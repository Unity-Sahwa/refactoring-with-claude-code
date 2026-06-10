using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WisuSuppressionController : Enemy
{
    private WisuMainRe wisuMain;
    private WisuSpawnPhase wisuSpawnPhase;
    public EnemyDataMelee ControllerData;
    public bool isInvincible = false;
    public ParticleSystem hpEffect;
    public ParticleSystem DieEffect;

    private float restoreHP;
    private float restoreHPCount;

    [Header("수정 비활성화시(파과) 이벤트")]
    public List<EventData> deactivateEvents;

    [Header("수정 활성화시 이벤트")]
    public List<EventData> activateEvents;


    protected override void Awake()
    {

    }

    protected override void Start()
    {
        restoreHP = hp;
        restoreHPCount = hpBarCount;

        wisuMain = FindObjectOfType<WisuMainRe>();
        wisuSpawnPhase = FindObjectOfType<WisuSpawnPhase>();     

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
        //Debug.Log($"제어기 현재 체력 : {phaseHPs[phaseHPs.Count - 1]}");
        //audioSourceHit.PlaySoundEffect(0);

        if (phaseHPs[phaseHPs.Count - 1] <= 0)
        {
            ClearHPBar();
        }

        return true;
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
