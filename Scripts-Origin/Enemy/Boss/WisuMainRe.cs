using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WisuMainRe : Enemy
{
    private WisuSpawnPhase spawnPhase;
    private WisuAttackPatternA1 patternA1;
    private WisuAttackPatternA2 patternA2;
    private WisuAttackPatternA3 patternA3;
    private WisuAttackPatternB1 patternB1;
    private WisuAttackPatternB2 patternB2;
    private WisuAttackPatternB3 patternB3;
    private BoxCollider boxCollider;

    [HideInInspector] public bool isPatternFinished = false; // 패턴 완료 플래그
    [HideInInspector] public bool isControllerActive = false; // 수정 제어기 부서짐 확인
    [HideInInspector] public bool isGroggy = false; // 그로기 상태 확인
    [HideInInspector] public bool isSpawnPhaseFinished = false; // 소환 패턴 종료 확인    
    [HideInInspector] public bool isGroggyRoutineRunning = false; // 코루틴 실행 여부 추적 변수
    [HideInInspector] public bool isInvincible = false;  // 무적상태 여부
    [HideInInspector] public float fullHP;
    [HideInInspector] public float currentHpValue; // hp바 부드럽게 하기 위한 변수
    [HideInInspector] public bool isHPphase = false;
    [HideInInspector] public int currentImageIndex = -1;   // 현재 활성화된 이미지 인덱스

    #region 보스 스텟
    [Header("<<<<<< 기본 스텟 >>>>>>")]
    public MainState mainstate;

    [Header("<<<<<< 맨손 기술 공격 A1 >>>>>>")]
    public Pattern_A1 pattern_A1;

    [Header("<<<<<< 맨손 기술 공격 A2 >>>>>>")]
    public Pattern_A2 pattern_A2;

    [Header("<<<<<< 맨손 기술 공격 A3 >>>>>>")]
    public Pattern_A3 pattern_A3;

    [Header("<<<<<< 무기 공격 B1 >>>>>>")]
    public Pattern_B1 pattern_B1;

    [Header("<<<<<< 무기 공격 B2 >>>>>>")]
    public Pattern_B2 pattern_B2;

    [Header("<<<<<< 무기 공격 B3 >>>>>>")]
    public Pattern_B3 pattern_B3;

    #region 소환 패턴
    [Header("<<<<<< 소환 패턴 >>>>>>")]
    [Header("1단계 소환 몬스터")]
    public List<GameObject> phase1Enemies = new List<GameObject>();
    [Header("1단계 소환 수")]
    public List<int> phase1Counts = new List<int>();
    [Header("1단계 각 소환 쿨타임")]
    public List<float> spawn1CoolTime = new List<float>();

    [Header("2단계 소환 몬스터")]
    public List<GameObject> phase2Enemies = new List<GameObject>();
    [Header("2단계 소환 수")]
    public List<int> phase2Counts = new List<int>();
    [Header("2단계 각 소환 쿨타임")]
    public List<float> spawn2CoolTime = new List<float>();

    [Header("3단계 소환 몬스터")]
    public GameObject phase3Enemy;
    [Header("3단계 소환 쿨타임")]
    public float spawn3CoolTime;

    [Header("소환 위치들")]
    public List<Transform> spawnPoints;

    [Header("각 단계 쿨타임")]
    public float delayBetweenSpawnPhase;

    #endregion

    #region 버프 패턴
    [Header("<<<<<< 버프 >>>>>>")]
    [Header("1. 버프 커지는 비율")]
    public float localScaleUp;
    #endregion

    [Header("<<<<<< 추가적으로 넣는 곳 >>>>>>")]
    public WisuSuppressionController suppressionController;
    public GameObject phase1Sword;
    public GameObject phase2Sword;
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject dangerZone_Bolt;
    public GameObject dangerZone_SmallPillar;
    public GameObject dangerZone_LargePillar;
    public GameObject dangerZone_B1;
    public Transform dangerZonePosition_B1;
    public Slider bossHPSlider;
    public GameObject bossHPObject;
    public float lerpSpeed;
    #endregion


    #region state
    public enum bState
    {
        ready,
        Idle,
        SpawnPhase,
        AttackPatternA1,
        AttackPatternA2,
        AttackPatternA3,
        AttackPatternB1,
        AttackPatternB2,
        AttackPatternB3,
        Die
    }
    private bState patternState;

    // 마지막 실행된 패턴을 저장할 변수
    private bState lastExecutedPattern = bState.Idle;
    private Transform lookatTransform;
    #endregion

    protected override void Awake()
    {

    }

    protected override void Start()
    {
        mainstate.phase = 1;

        patternA1 = GetComponent<WisuAttackPatternA1>();
        patternA2 = GetComponent<WisuAttackPatternA2>();
        patternA3 = GetComponent<WisuAttackPatternA3>();
        patternB1 = GetComponent<WisuAttackPatternB1>();
        patternB2 = GetComponent<WisuAttackPatternB2>();
        patternB3 = GetComponent<WisuAttackPatternB3>();
        spawnPhase = GetComponent<WisuSpawnPhase>();

        target = GameObject.FindWithTag("PlayerScript").GetComponent<Player>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = false;

        mainstate.phase2Rate /= 100;
        mainstate.phase3Rate /= 100;

        patternState = bState.ready;
    }

    protected override void Update()
    {
        if (patternState == bState.ready)
        {
            if (isHPphase)
            {
                currentHpValue = Mathf.Lerp(currentHpValue, fullHP, Time.deltaTime * lerpSpeed);
                bossHPSlider.value = currentHpValue;
            }
            return;
        }
        //Debug.Log(fullHP);
        currentHpValue = Mathf.Lerp(currentHpValue, fullHP, Time.deltaTime * lerpSpeed);
        bossHPSlider.value = currentHpValue;



        //일정 이상 체력 까이면 시네머신 재생
        if (mainstate.phase == 1 && fullHP < hp * mainstate.phase2Rate)
        {
            mainstate.phase++;
            StopAllCoroutines();
            AfterGroggy();
            isInvincible = true;
            patternState = bState.SpawnPhase;
            StartCoroutine(BossStateManager());
        }
        //else if (phase == 2 && fullHP < hp * phase3Rate || hpBarCount == 1)
        //{
        //    StopAllCoroutines();
        //    isPatternFinished = true;
        //    phase++;
        //}

        if (isControllerActive)
        {
            isControllerActive = false;
            StopPattern();
            if (patternState == bState.SpawnPhase)
            {
                return;
            }
            StopAllCoroutines();
            animator.speed = 1;
            animator.Play("groggy3", -1, 0f);
            animator.SetBool("GroggyTime", true);

            patternState = bState.Idle;
        }

        if (isSpawnPhaseFinished)
        {
            isInvincible = false;
            isSpawnPhaseFinished = false;
            animator.SetBool("SpawnTime", false);
            animator.SetTrigger("SecondPhaseStart");
            isPatternFinished = true;
            patternState = bState.Idle;
        }

        if (isPatternFinished)
        {
            //보스 패턴 선택
            isPatternFinished = false;
            bState nextPattern = GetRandomPattern(mainstate.phase);
            lastExecutedPattern = nextPattern;
            patternState = nextPattern;

            //patternState = bState.AttackPatternB3;

            StartCoroutine(BossStateManager());
        }
    }
    protected override void FixedUpdate()
    {

    }

    public void StopPattern()
    {
        patternA1.StopPattern();
        patternA2.StopPattern();
        patternA3.StopPattern();
        patternB1.StopPattern();
        patternB2.StopPattern();
        patternB3.StopPattern();
    }

    public void ActiveSword()
    {
        phase1Sword.SetActive(false);
        phase2Sword.SetActive(true);
    }

    public void BossGroggyStart()
    {
        if (isDead)
        {
            return;
        }
        StartCoroutine(BossGroggy());
    }

    #region 상태 관리
    public IEnumerator BossStateManager()
    {
        yield return new WaitForSeconds(mainstate.coolTime);
        switch (patternState)
        {
            case bState.Idle:
                isPatternFinished = true;
                break;
            case bState.SpawnPhase:
                animator.SetBool("SpawnTime", true);
                spawnPhase.StartPattern();
                break;
            case bState.AttackPatternA1:
                animator.SetTrigger("A1");
                break;

            case bState.AttackPatternA2:
                animator.SetTrigger("A2");
                break;

            case bState.AttackPatternA3:
                animator.SetTrigger("A3");
                break;

            case bState.AttackPatternB1:
                animator.SetTrigger("B1");
                break;

            case bState.AttackPatternB2:
                animator.SetTrigger("B2");
                break;

            case bState.AttackPatternB3:
                animator.SetTrigger("B3");
                break;
        }
    }
    #endregion

    #region 패턴 실행

    public void StartPatternA1()
    {
        patternA1.StartPattern();
    }
    public void StartPatternA2()
    {
        patternA2.StartPattern();
    }
    public void StartPatternA3()
    {
        patternA3.StartPattern();
    }
    public void StartPatternB1()
    {
        patternB1.StartPattern();
    }
    public void StartPatternB2()
    {
        patternB2.StartPattern();
    }
    public void StartPatternB3()
    {
        patternB3.StartPattern();
    }
    #endregion

    #region 공격 범위 표시
    public void DisplayDangerZoneA1()
    {

    }

    public void DisplayDangerZoneB1()
    {
        GameObject dangerZoneInst = Instantiate(dangerZone_Bolt, dangerZonePosition_B1.position, Quaternion.identity);
    }
    #endregion


    private bState GetRandomPattern(float num)
    {
        bState[] randomPatterns;

        if (mainstate.phase == 1)
        {
            randomPatterns = new bState[]
            {
                bState.AttackPatternA1,
                bState.AttackPatternA2,
                bState.AttackPatternA3
            };
        }
        else
        {
            randomPatterns = new bState[]
            {
                bState.AttackPatternB1,
                bState.AttackPatternB2,
                bState.AttackPatternB3
            };
        }

        // 마지막 실행된 패턴을 제외한 패턴 목록 생성
        List<bState> availablePatterns = new List<bState>(randomPatterns);
        availablePatterns.Remove(lastExecutedPattern);

        // 랜덤으로 새로운 패턴 선택
        int randomIndex = Random.Range(0, availablePatterns.Count);
        return availablePatterns[randomIndex];
    }

    public IEnumerator BossGroggy()
    {
        boxCollider.enabled = true;
        isGroggy = true;
        yield return new WaitForSeconds(mainstate.groggyTime);

        AfterGroggy();

        yield return new WaitForSeconds(3f);

        isPatternFinished = true;
        suppressionController.ResetController();
    }

    public void AfterGroggy()
    {
        isControllerActive = false;
        boxCollider.enabled = false;
        animator.SetBool("GroggyTime", false);
        isGroggy = false;
    }

    public void BossStageStart()
    {
        patternState = bState.Idle;
        StartCoroutine(BossStateManager());
    }

    public void HPUIOn()
    {
        fullHP = hp;
        phaseHPs = new List<float>();
        phaseHP = hp / hpBarCount;
        for (var i = 0; i < hpBarCount; i++)
        {
            phaseHPs.Add(phaseHP);
        }

        bossHPObject.SetActive(true);
        bossHPSlider.maxValue = fullHP;
        isHPphase = true;
    }

    public void AnimationSpeed(float speed)
    {
        animator.speed = speed;
    }
    public void ShowBossPatternImage(int index)
    {
        int imageIndex = index + 1;

        if (imageIndex == 0) // index가 0이면 모든 이미지를 비활성화
        {
            HideAllImages();
            return;
        }

        // 리스트 범위 확인 후 특정 이미지 활성화
        if (imageIndex > 0 && imageIndex <= mainstate.bossPatternImages.Count)
        {
            // 새로운 이미지 활성화
            currentImageIndex = imageIndex - 1;
            mainstate.bossPatternImages[currentImageIndex].gameObject.SetActive(true);

            // 일정 시간 후 해당 이미지를 비활성화
            StartCoroutine(HideImageAfterDuration(mainstate.displayDuration));
        }
    }

    private IEnumerator HideImageAfterDuration(float duration)
    {
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 모든 이미지를 비활성화
        HideAllImages();
    }

    private void HideAllImages()
    {
        foreach (var image in mainstate.bossPatternImages)
        {
            if (image != null)
            {
                image.gameObject.SetActive(false);
            }
        }
        currentImageIndex = -1;
    }


    #region 데미지 받을때
    public override bool ApplyDamage(DamageMessage damageMessage)
    {
        //TODO: [대원] 죽을 때 들어오면 버그 발생
        if (fullHP <= 0 || phaseHPs[0] <= 0.5f)
        {
            return false;
        }
        if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || damageMessage.damager == gameObject || isDead || isInvincible)
        {
            return false;
        }

        fullHP--;
        phaseHPs[phaseHPs.Count-1] -= damageMessage.amount;
        lastDamagedTime = Time.time;
        //audioSourceHit.PlaySoundEffect(0);

        
        //TODO: [대원] HP가 0에 도달했을 때 바로 죽음처리
        //phaseHPs[phaseHPs.Count - 1]는 float임 0에 가까워지다가 다음 인덱스로 넘어가고 0에 가까워진 값은 원상복구됨 -> 그래서 체력 HUD 제대로 반영이 안됨
        if (fullHP <= 0 || phaseHPs[0] <= 0.5f)
        {
            ClearBossHPBar();
            return true;
        }


        //if (phaseHPs[phaseHPs.Count - 1] < 1)
        //{
        //    ClearBossHPBar();
        //}

        
        if (hpBarCount > 0 && damageMessage.amount != 0)
        {
            if (calliSystem != null)
            {
                if (!stackUI.activeSelf) stackUI.SetActive(true);
                calliSystem.Painting(damageMessage.color, damageMessage.value);

                if (calliSystem.paintOver < calliSystem.MaxPaintOver)
                {
                    for (var i = 0; i < calliSystem.paintOver + 1; i++)
                    {
                        paintOverStacks[i].SetActive(true);
                    }
                }
                else
                {
                    for (var i = 0; i < calliSystem.MaxPaintOver + 1; i++)
                    {
                        paintOverStacks[i].SetActive(true);
                        paintOverStacks[i].GetComponent<Image>().color = Color.white;
                    }
                    paintOverMax = true;
                }
            }
        }
        return true;
    }
    #endregion

    #region 처형 당할때
    public override void Execution()
    {
        if (paintOverMax && !isDead && isGroggy)
        {
            //todo 처형 당하는 모션과 사운드
            ClearHPBar();
        }
    }
    #endregion

    #region 사망처리
    public override void DieAction()
    {
        Debug.Log("Wisu Die");

        StopAllCoroutines();
        animator.SetBool("GroggyTime", false);
        animator.SetTrigger("Die");

        if (isDead)
        {
            return;
        }
        isDead = true;
        //audioSourceDie.PlaySoundEffect(0);
        phaseHPs.Clear();
        stackUI.SetActive(false);

        //if (isKillTrigger)
        //{
        //    while (delayTimes.Count < DeathEvent.Count)
        //    {
        //        delayTimes.Add(0f);
        //    }
        //    for (int i = 0; i < DeathEvent.Count; i++)
        //    {
        //        if (DeathEvent[i] != null)
        //        {
        //            StartCoroutine(ExecuteEventWithDelay(DeathEvent[i], delayTimes[i]));
        //        }
        //    }
        //}

        UIEffect.instance.StartCoroutine(UIEffect.instance.ShowBossDefeatedScreen());
    }
    public void DestroyBoss()
    {
        //Destroy(gameObject);
    }
    #endregion
}


#region 클래스

#region main
[System.Serializable]
public class MainState
{
    [Header("1. 그로기 시간")]
    public float groggyTime;

    [Header("2. 2페이즈 체력비율")]
    public float phase2Rate;

    [Header("3. 3페이즈 체력비율")]
    public float phase3Rate;

    [Header("4. 패턴 간 쿨타임")]
    public float coolTime;

    [Header("5. 경고 이미지")]
    public List<Image> bossPatternImages;

    [Header("6. 경고 이미지 표시 시간")]
    public float displayDuration;

    //페이즈 단계
    [Header("페이즈 단계")]
    public float phase;
}
#endregion

#region 맨손 공격_A1
[System.Serializable]
public class Pattern_A1
{
    [Header("1. 불꽃 발사체 프리팹")]
    public List<GameObject> A1_prefabs;

    [Header("2. 발사하는 곳")]
    public List<Transform> A1_points;

    [Header("3. 발사 간격")]
    public List<float> A1_intervals;

    [Header("4. 기본 발사 간격")]
    public float A1_defaultInterval;

    [Header("5. 모든 공격 완료하고 대기시간")]
    public float A1_waitingTime;
}
#endregion

#region 맨손 공격_A2
[System.Serializable]
public class Pattern_A2
{
    [Header("1. 불기둥 프리팹")]
    public List<GameObject> A2_prefabs;

    [Header("2. 생성되는 곳")]
    public List<Transform> A2_points;

    [Header("3. 발사 간격")]
    public List<float> A2_intervals;

    [Header("4. 기본 발사 간격")]
    public float A2_defaultInterval;

    [Header("5. 모든 공격 완료하고 대기시간")]
    public float A2_waitingTime;
}
#endregion

#region 맨손 공격_A3
[System.Serializable]
public class Pattern_A3
{
    [Header(" 구역 1 ")]
    [Header("1. 구역 설정")]
    public List<Transform> A3_area1;

    [Header("2. 불기둥 프리팹")]
    public GameObject A3_area1_prefab;

    [Header("3. 생성 대기 시간")]
    public float A3_area1_Interval;

    [Header(" 구역 2 ")]
    [Header("1. 구역 설정")]
    public List<Transform> A3_area2;

    [Header("2. 불기둥 프리팹")]
    public GameObject A3_area2_prefab;

    [Header("3. 생성 대기 시간")]
    public float A3_area2_Interval;

    [Header(" 구역 3 ")]
    [Header("1. 구역 설정")]
    public List<Transform> A3_area3;

    [Header("2. 불기둥 프리팹")]
    public GameObject A3_area3_prefab;

    [Header("3. 생성 대기 시간")]
    public float A3_area3_Interval;

    [Header(" 구역 4 ")]
    [Header("1. 구역 설정")]
    public List<Transform> A3_area4;

    [Header("2. 불기둥 프리팹")]
    public GameObject A3_area4_prefab;

    [Header("3. 생성 대기 시간")]
    public float A3_area4_Interval;

    [Header(" 구역 5 ")]
    [Header("1. 구역 설정")]
    public List<Transform> A3_area5;

    [Header("2. 불기둥 프리팹")]
    public GameObject A3_area5_prefab;

    [Header("3. 생성 대기 시간")]
    public float A3_area5_Interval;


    [Header("모든 공격 완료하고 대기시간")]
    public float A3_waitingTime;

}
#endregion

#region 무기 공격_B1
[System.Serializable]
public class Pattern_B1
{
    [Header(" 구역 1 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B1_area1;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B1_area1_prefab;

    [Header("3. 생성 대기 시간")]
    public float B1_area1_Interval;

    [Header(" 구역 2 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B1_area2;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B1_area2_prefab;

    [Header("3. 생성 대기 시간")]
    public float B1_area2_Interval;

    [Header(" 구역 3 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B1_area3;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B1_area3_prefab;

    [Header("3. 생성 대기 시간")]
    public float B1_area3_Interval;

    [Header(" 구역 4 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B1_area4;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B1_area4_prefab;

    [Header("3. 생성 대기 시간")]
    public float B1_area4_Interval;

    [Header(" 구역 5 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B1_area5;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B1_area5_prefab;

    [Header("3. 생성 대기 시간")]
    public float B1_area5_Interval;

    [Header("모든 공격 완료하고 대기시간")]
    public float B1_waitingTime;
}
#endregion

#region 무기 공격_B2
[System.Serializable]
public class Pattern_B2
{
    [Header(" 구역 1 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B2_area1;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B2_area1_prefab;

    [Header("3. 생성 대기 시간")]
    public float B2_area1_Interval;

    [Header(" 구역 2 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B2_area2;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B2_area2_prefab;

    [Header("3. 생성 대기 시간")]
    public float B2_area2_Interval;

    [Header(" 구역 3 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B2_area3;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B2_area3_prefab;

    [Header("3. 생성 대기 시간")]
    public float B2_area3_Interval;

    [Header(" 구역 4 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B2_area4;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B2_area4_prefab;

    [Header("3. 생성 대기 시간")]
    public float B2_area4_Interval;


    [Header(" 구역 5 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B2_area5;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B2_area5_prefab;

    [Header("3. 생성 대기 시간")]
    public float B2_area5_Interval;

    [Header(" 구역 6 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B2_area6;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B2_area6_prefab;

    [Header("3. 생성 대기 시간")]
    public float B2_area6_Interval;

    [Header(" 구역 7 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B2_area7;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B2_area7_prefab;

    [Header("3. 생성 대기 시간")]
    public float B2_area7_Interval;

    [Header(" 구역 8 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B2_area8;

    [Header("2. 강화 불기둥 프리팹")]
    public GameObject B2_area8_prefab;

    [Header("3. 생성 대기 시간")]
    public float B2_area8_Interval;

    [Header("모든 공격 완료하고 대기시간")]
    public float B2_waitingTime;
}
#endregion

#region 무기 공격_B3
[System.Serializable]
public class Pattern_B3
{
    [Header(" 구역 1 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B3_area1;

    [Header("2. 불기둥 프리팹")]
    public GameObject B3_area1_prefab;

    [Header("3. 생성 대기 시간")]
    public float B3_area1_Interval;

    [Header(" 구역 2 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B3_area2;

    [Header("2. 불기둥 프리팹")]
    public GameObject B3_area2_prefab;

    [Header("3. 생성 대기 시간")]
    public float B3_area2_Interval;

    [Header(" 구역 3 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B3_area3;

    [Header("2. 불기둥 프리팹")]
    public GameObject B3_area3_prefab;

    [Header("3. 생성 대기 시간")]
    public float B3_area3_Interval;

    [Header(" 구역 4 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B3_area4;

    [Header("2. 불기둥 프리팹")]
    public GameObject B3_area4_prefab;

    [Header("3. 생성 대기 시간")]
    public float B3_area4_Interval;

    [Header(" 구역 5 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B3_area5;

    [Header("2. 불기둥 프리팹")]
    public GameObject B3_area5_prefab;

    [Header("3. 생성 대기 시간")]
    public float B3_area5_Interval;

    [Header("모든 공격 완료하고 대기시간")]
    public float B3_waitingTime;

    [Header(" 구역 6 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B3_area6;

    [Header("2. 불기둥 프리팹")]
    public GameObject B3_area6_prefab;

    [Header("3. 생성 대기 시간")]
    public float B3_area6_Interval;

    [Header(" 구역 7 ")]
    [Header("1. 구역 설정")]
    public List<Transform> B3_area7;

    [Header("2. 불기둥 프리팹")]
    public GameObject B3_area7_prefab;

    [Header("3. 생성 대기 시간")]
    public float B3_area7_Interval;
}
#endregion

#endregion