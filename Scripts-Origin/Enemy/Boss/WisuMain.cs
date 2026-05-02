//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor.Build;
//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.UI;
//using static UnityEngine.Rendering.DebugUI;

//public class WisuMain : MonoBehaviour
//{
//    [HideInInspector] public Player player;
//    private WisuSpawnPhase spawnEnemy;
//    private WisuAttackPatternA1 fireBolt;
//    private WisuAttackPatternA2 firePillar;
//    private WisuAttackPatternA3 groundAttack1;
//    private WisuAttackPatternB1 groundAttack2;
//    private WisuAttackPatternB2 groundAttack3;
//    private WisuAttackPatternB3 groundAttack4;
//    [HideInInspector] public Animator animator;
//    private BoxCollider boxCollider;

//    private List<IWisuPattern> patterns = new List<IWisuPattern>();


//    #region 보스 스텟
//    [Header("<<<<<< 보스 스텟 >>>>>>")]
//    [Header("1. 체력")]
//    public float hp;

//    [Header("2. 체력바")]
//    public float hpBarCount;

//    [Header("3. 그로기 시간")]
//    public float groggyTime;

//    [Header("4. 2페이즈 체력비율")]
//    public float phase2Rate;

//    [Header("5. 3페이즈 체력비율")]
//    public float phase3Rate;

//    [Header("6. 패턴 간 쿨타임")]
//    public float coolTime;


//    #region spawnEnemy
//    [Header("<<<<<< 몬스터 소환 패턴 >>>>>>")]
//    [Header("1. 몬스터 소환 위치")]
//    public List<Transform> spawnPoints;

//    [Header("2. 소환 페이즈 대기시간")]
//    public float delayBetweenSpawn;

//    [Header("3. 소환 간격")]
//    public float spawnSpeed;
//    #endregion

//    #region WisuBuff
//    [Header("<<<<<< 버프 >>>>>>")]
//    [Header("1. 버프 커지는 비율")]
//    public float localScaleUp;
//    #endregion

//    #region Magic 1
//    [Header("<<<<<< 맨손 공격 A1 >>>>>>")]
//    [Header("1. 불꽃 발사체 프리팹")]
//    public GameObject FireBoltPrefab;

//    [Header("2. 발사하는 곳")]
//    public Transform firePoint;

//    [Header("3. 날아가는 속도")]
//    public float projectileSpeed;

//    [Header("4. 데미지")]
//    public float fireBoltDamage;

//    [Header("5. 발사체 생존 시간")]
//    public float fireBoltLifetime;
//    #endregion

//    #region Magic 2
//    [Header("<<<<<< 맨손 공격 A2 >>>>>>")]
//    [Header("1. 불기둥 프리팹")]
//    public GameObject FirePillarPrefab;

//    [Header("2. 생성되는 곳")]
//    public Transform ActivePoint;

//    [Header("3. 불기둥 속도")]
//    public float firePillarSpeed;

//    [Header("4. 불기둥 데미지")]
//    public float firePillarDamage;

//    [Header("5. 불기둥 시간")]
//    public float firePillarLifetime;
//    #endregion

//    #region GroundAttack 1
//    [Header("<<<<<< 맨손 공격 A3 >>>>>>")]
//    [Header("1. 1 구역")]
//    public List<Transform> GroundAttack1_Area1;

//    [Header("2. 2 구역")]
//    public List<Transform> GroundAttack1_Area2;

//    [Header("3. 3 구역")]
//    public List<Transform> GroundAttack1_Area3;

//    [Header("4. 불기둥 시간")]
//    public float GroundAttack1_Lifetime;

//    [Header("5. 불기둥 간격")]
//    public float GroundAttack1_interval;
//    #endregion

//    #region GroundAttack 2
//    [Header("<<<<<< 그라운드 어택 2 패턴 >>>>>>")]
//    [Header("1. 1 구역")]
//    public List<Transform> GroundAttack2_Area1;

//    [Header("2. 2 구역")]
//    public List<Transform> GroundAttack2_Area2;

//    [Header("3. 3 구역")]
//    public List<Transform> GroundAttack2_Area3;

//    [Header("4. 4 구역")]
//    public List<Transform> GroundAttack2_Area4; 

//    [Header("5. 5 구역")]
//    public List<Transform> GroundAttack2_Area5;

//    [Header("4. 불기둥 시간")]
//    public float GroundAttack2_Lifetime;

//    [Header("5. 불기둥 간격")]
//    public float GroundAttack2_interval;

//    [Header("6. 불기둥2 프리팹")]
//    public GameObject FirePillarPrefab2;
//    #endregion

//    #region GroundAttack 3
//    [Header("<<<<<< 그라운드 어택 3 패턴 >>>>>>")]
//    [Header("1. 1 구역")]
//    public List<Transform> GroundAttack3_Area1;

//    [Header("2. 2 구역")]
//    public List<Transform> GroundAttack3_Area2;

//    [Header("3. 3 구역")]
//    public List<Transform> GroundAttack3_Area3;

//    [Header("4. 4 구역")]
//    public List<Transform> GroundAttack3_Area4;

//    [Header("5. 5 구역")]
//    public List<Transform> GroundAttack3_Area5;

//    [Header("4. 불기둥 시간")]
//    public float GroundAttack3_Lifetime;

//    [Header("5. 불기둥 간격")]
//    public float GroundAttack3_interval;
//    #endregion

//    #region GroundAttack 4
//    [Header("<<<<<< 그라운드 어택 4 패턴 >>>>>>")]
//    [Header("1. 1 구역")]
//    public List<Transform> GroundAttack4_Area1;

//    [Header("2. 2 구역")]
//    public List<Transform> GroundAttack4_Area2;

//    [Header("3. 3 구역")]
//    public List<Transform> GroundAttack4_Area3;

//    [Header("4. 4 구역")]
//    public List<Transform> GroundAttack4_Area4;

//    [Header("5. 5 구역")]
//    public List<Transform> GroundAttack4_Area5;

//    [Header("4. 불기둥 시간")]
//    public float GroundAttack4_Lifetime;

//    [Header("5. 불기둥 간격")]
//    public float GroundAttack4_interval;
//    #endregion


//    //피격 무적 시간
//    protected const float MIN_TIME_BET_DAMAGE = 0.1f;

//    //최근 (플레이어로 부터)데미지 당한 시각
//    protected float lastDamagedTime = 0;

//    //페이즈 단계
//    [Header("페이즈 단계")]
//    public float phase = 2;
//    [HideInInspector] public bool bossLookedPlayer = false;
    
//    //그로기 상태 확인
//    [HideInInspector] public bool isGroggy = false;

//    //무적 상태
//    [HideInInspector] public bool powerOverwhelming =  false;
//    #endregion

//    //체력 변수
//    [HideInInspector] public List<float> phaseHPs;

//    //사망 상태 확인
//    [HideInInspector] public bool isDead = false;

//    // 코루틴 실행 여부 추적 변수
//    private bool isGroggyRoutineRunning = false;

//    #region 덧칠 시스템
//    [Header("<<<<<< 덧칠 시스템 >>>>>>")]
//    protected CalliSystem calliSystem;
//    public GameObject[] paintOverStacks;
//    public GameObject stackUI;
//    protected bool paintOverMax;
//    #endregion

//    #region state
//    public enum bState
//    {
//        ready,
//        Idle,
//        Groggy,
//        SpawnEnemy1,
//        SpawnEnemy2,
//        Firebolt,
//        FirePillar,
//        GroundAttack1,
//        GroundAttack2,
//        GroundAttack3,
//        GroundAttack4,
//        Hit
//    }
//    private bState patternState;

//    // 마지막 실행된 패턴을 저장할 변수
//    private bState lastExecutedPattern;
//    private Transform lookatTransform;

//    #endregion

//    #region Start()
//    void Start()
//    {        
//        player = GameObject.FindWithTag("PlayerScript").GetComponent<Player>();
//        animator = GetComponent<Animator>();
//        boxCollider = GetComponent<BoxCollider>();
//        boxCollider.enabled = false;
//        lookatTransform = transform.Find("Lookat");
//        phase2Rate /= 100;
//        phase3Rate /= 100;


//        phaseHPs = new List<float>();
//        phaseHP = hp / hpBarCount;
//        for (var i = 0; i < hpBarCount; i++)
//        {
//            phaseHPs.Add(phaseHP);
//        }

//        //spawnEnemy = GetComponent<WisuSpawnEnemy>();
//        //spawnEnemy.Initalize(this);
        
//        //fireBolt = GetComponent<WisuFireBolt>();
//        //fireBolt.Initalize(this);
//        //patterns.Add(fireBolt);

//        //firePillar = GetComponent<WisuFirePillar>();
//        //firePillar.Initalize(this);
//        //patterns.Add(firePillar);

//        //groundAttack1 = GetComponent<WisuGroundAttack1>();
//        //groundAttack1.Initalize(this);
//        //patterns.Add(groundAttack1);

//        //groundAttack2 = GetComponent<WisuGroundAttack2>();
//        //groundAttack2.Initalize(this);
//        //patterns.Add(groundAttack2);

//        //groundAttack3 = GetComponent<WisuGroundAttack3>();
//        //groundAttack3.Initalize(this);
//        //patterns.Add(groundAttack3);

//        //groundAttack4 = GetComponent<WisuGroundAttack4>();
//        //groundAttack4.Initalize(this);
//        //patterns.Add(groundAttack4);
//    }
//    #endregion
    
//    void Update()
//    {
//        Debug.Log(patternState);
//        if(bossLookedPlayer)
//        {
//            LookAtPlayer();
//        }

//        if (isGroggy && !isGroggyRoutineRunning)
//        {
//            patternState = bState.Groggy;
//            StartCoroutine(GroggyRoutine());
//        }

//        if (phase == 1 && hp < hp * phase2Rate)
//        {
//            patternState = bState.SpawnEnemy1;
//            phase++;
//        }
//        else if (phase == 2 && hp < hp * phase3Rate)
//        {
//            patternState = bState.SpawnEnemy2;
//            phase++;
//        }

//        if (patternState != bState.Idle)
//        {
//            if (patternState == bState.SpawnEnemy1 && spawnEnemy.isPatternFinished)
//            {
//                bossLookedPlayer = false;
//                spawnEnemy.isPatternFinished = false;
//                patternState = bState.Idle;
//                animator.SetBool("Spawn", false);
//            }
//            else if (patternState == bState.SpawnEnemy2 && spawnEnemy.isPatternFinished)
//            {
//                bossLookedPlayer = false;
//                spawnEnemy.isPatternFinished = false;
//                patternState = bState.Idle;
//                animator.SetBool("Spawn", false);
//            }
//            else if (patternState == bState.Firebolt && fireBolt.isPatternFinished)
//            {
//                bossLookedPlayer = false;
//                fireBolt.isPatternFinished = false;
//                patternState = bState.Idle;
//                animator.SetBool("FireBolt", false);
//            }
//            else if (patternState == bState.FirePillar && firePillar.isPatternFinished)
//            {
//                bossLookedPlayer = false;
//                firePillar.isPatternFinished = false;
//                patternState = bState.Idle;
//                animator.SetBool("FirePillar", false);
//            }
//            else if (patternState == bState.GroundAttack1 && groundAttack1.isPatternFinished)
//            {
//                bossLookedPlayer = false;
//                groundAttack1.isPatternFinished = false;
//                patternState = bState.Idle;
//                animator.SetBool("GroundAttack1", false);
//            }
//            else if (patternState == bState.GroundAttack2 && groundAttack2.isPatternFinished)
//            {
//                bossLookedPlayer = false;
//                groundAttack2.isPatternFinished = false;
//                patternState = bState.Idle;
//                animator.SetBool("GroundAttack2", false);
//            }
//            else if (patternState == bState.GroundAttack3 && groundAttack3.isPatternFinished)
//            {
//                bossLookedPlayer = false;
//                groundAttack3.isPatternFinished = false;
//                patternState = bState.Idle;
//                animator.SetBool("GroundAttack3", false);
//            }
//            else if (patternState == bState.GroundAttack4 && groundAttack4.isPatternFinished)
//            {
//                bossLookedPlayer = false;
//                groundAttack4.isPatternFinished = false;
//                patternState = bState.Idle;
//                animator.SetBool("GroundAttack4", false);
//            }
//        }
//        else
//        {    
//            if(phase == 1)
//            {
//                StartCoroutine(PatternWaiting());
//                patternState = Phase1_Pattern(); // 다음 패턴을 랜덤으로 설정
//            }
//            else if (phase == 2 || phase == 3)
//            {
//                patternState = Phase2_Pattern(); // 다음 패턴을 랜덤으로 설정
//            }
//            #region 스위치
//            switch (patternState)
//            {
//                case bState.Firebolt:
//                    bossLookedPlayer = true;
//                    fireBolt.StartPattern();
//                    break;
//                case bState.FirePillar:                 
//                    bossLookedPlayer = true;
//                    firePillar.StartPattern();
//                    break;
//                case bState.GroundAttack1:
//                    groundAttack1.StartPattern();
//                    break;

//                case bState.GroundAttack2:
//                    bossLookedPlayer = true;
//                    groundAttack2.StartPattern();
//                    break;
//                case bState.GroundAttack3:
//                    bossLookedPlayer = true;
//                    groundAttack3.StartPattern();
//                    break;
//                case bState.GroundAttack4:
//                    groundAttack4.StartPattern();
//                    break;
//            }
//            #endregion
//            lastExecutedPattern = patternState; // 마지막 실행된 패턴을 저장
//        }
//    }

//    public void BossStageStart()
//    {
//        if (patternState == bState.ready)
//        {
//            animator.SetTrigger("1PhaseStart");
//        }
//    }

//    public void BossPatternIdle()
//    {
//        patternState = bState.Idle;
//    }

//    #region 페이즈 공격 랜덤뽑기

//    private bState Phase1_Pattern()
//    {
//        List<bState> possiblePatterns = new List<bState> { bState.Firebolt, bState.FirePillar, bState.GroundAttack1 };

//        // 이전 패턴을 제외한 나머지 패턴 중 하나를 선택
//        possiblePatterns.Remove(lastExecutedPattern);

//        int randomIndex = Random.Range(0, possiblePatterns.Count);
//        Debug.Log(possiblePatterns[randomIndex]);
//        return possiblePatterns[randomIndex];
        
//    }

//    private bState Phase2_Pattern()
//    {
//        List<bState> possiblePatterns = new List<bState> { bState.GroundAttack2, bState.GroundAttack3, bState.GroundAttack4 };

//        // 이전 패턴을 제외한 나머지 패턴 중 하나를 선택
//        possiblePatterns.Remove(lastExecutedPattern);

//        int randomIndex = Random.Range(0, possiblePatterns.Count);
//        return possiblePatterns[randomIndex];
//    }
//    #endregion

//    #region 데미지 받을때
//    public virtual bool ApplyDamage(DamageMessage damageMessage)
//    {
//        if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || damageMessage.damager == gameObject || isDead)
//        {
//            return false;
//        }

//        phaseHPs[phaseHPs.Count - 1] -= damageMessage.amount;
//        lastDamagedTime = Time.time;

//        //audioSourceHit.PlaySoundEffect(0);

//        if (phaseHPs[phaseHPs.Count - 1] <= 0)
//        {
//            ClearHPBar();
//        }

//        if (hpBarCount > 0 && damageMessage.amount != 0)
//        {
//            if (calliSystem != null)
//            {
//                if (!stackUI.activeSelf) stackUI.SetActive(true);
//                calliSystem.Painting(damageMessage.color, damageMessage.value);

//                if (calliSystem.paintOver < calliSystem.MaxPaintOver)
//                {
//                    for (var i = 0; i < calliSystem.paintOver + 1; i++)
//                    {
//                        paintOverStacks[i].SetActive(true);
//                    }
//                }
//                else
//                {
//                    for (var i = 0; i < calliSystem.MaxPaintOver + 1; i++)
//                    {
//                        paintOverStacks[i].SetActive(true);
//                        paintOverStacks[i].GetComponent<Image>().color = Color.white;
//                    }
//                    paintOverMax = true;
//                }
//            }
//        }
//        return true;
//    }
//    #endregion

//    #region 처형 당할때
//    public void Execution()
//    {
//        if (paintOverMax && !isDead && isGroggy)
//        {
//            //todo 처형 당하는 모션과 사운드
//            ClearHPBar();
//        }
//    }
//    #endregion

//    #region 체력 바 감소
//    public void ClearHPBar()
//    {
//        phaseHPs.RemoveAt(phaseHPs.Count - 1);
//        hpBarCount--;
//        if (hpBarCount <= 0)
//        {
//            DieAction();
//        }
//    }
//    #endregion

//    #region 사망처리
//    public virtual void DieAction()
//    {
//        StopAllCoroutines();
//        animator.SetTrigger("Die");
//        if (isDead)
//        {
//            return;
//        }

//        isDead = true;
//        //audioSourceDie.PlaySoundEffect(0);
//        phaseHPs.Clear();
//        stackUI.SetActive(false);

//        //
//        //if (isKillTrigger)
//        //{
//        //    while (delayTimes.Count < DeathEvent.Count)
//        //    {
//        //        delayTimes.Add(0f);
//        //    }
//        //    for (int i = 0; i < DeathEvent.Count; i++)
//        //    {
//        //        if (DeathEvent[i] != null)
//        //        {
//        //            StartCoroutine(ExecuteEventWithDelay(DeathEvent[i], delayTimes[i]));
//        //        }
//        //    }
//        //}
//    }
//    #endregion

//    public void StopAllPatterns()
//    {
//        foreach (IWisuPattern pattern in patterns)
//        {
//            pattern.StopPattern();
//        }
//        animator.SetBool("Firebolt", false);
//        animator.SetBool("FirePillar", false);
//        animator.SetBool("GroundAttack1", false);
//        animator.SetBool("GroundAttack2", false);
//        animator.SetBool("GroundAttack3", false);
//        animator.SetBool("GroundAttack4", false);
//        isGroggy = true;
//    }

//    public void LookAtPlayer()
//    {
//        // 플레이어를 바라보도록 설정
//        lookatTransform.transform.LookAt(player.transform.position);
//    }
//    private IEnumerator GroggyRoutine()
//    {   
//        animator.SetBool("Groggy", true); // 그로기 애니메이션 루프 시작
//        boxCollider.enabled = true;
//        float elapsedTime = 0f;
//        while (elapsedTime < groggyTime)
//        {
//            elapsedTime += Time.deltaTime;
//            yield return null; // 매 프레임 대기
//        }

//        boxCollider.enabled = false;
//        animator.SetBool("Groggy", false); // 그로기 애니메이션 중지

//        isGroggy = false;
//        yield return new WaitForSeconds(1f);

//        patternState = bState.Idle;
//    }
//    private IEnumerator PatternWaiting()
//    {
//        yield return new WaitForSeconds(coolTime);
//    }

//}
