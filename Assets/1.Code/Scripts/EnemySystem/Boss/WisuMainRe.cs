using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Refactoring
{
public class WisuMainRe : Enemy
{
    [Preserve, Inject] ICurrentCharacterProvider currentCharacterProvider;
    private WisuSpawnPhase spawnPhase;
    private WisuAttackPatternA1 patternA1;
    private WisuAttackPatternA2 patternA2;
    private WisuAttackPatternA3 patternA3;
    private WisuAttackPatternB1 patternB1;
    private WisuAttackPatternB2 patternB2;
    private WisuAttackPatternB3 patternB3;
    private BoxCollider boxCollider;

    [HideInInspector] public bool isPatternFinished = false;
    [HideInInspector] public bool isControllerActive = false;
    [HideInInspector] public bool isGroggy = false;
    [HideInInspector] public bool isSpawnPhaseFinished = false;
    [HideInInspector] public bool isGroggyRoutineRunning = false;
    [HideInInspector] public bool isInvincible = false;
    [HideInInspector] public float fullHP;
    [HideInInspector] public float currentHpValue;
    [HideInInspector] public bool isHPphase = false;
    [HideInInspector] public int currentImageIndex = -1;

    public MainState mainstate;
    public Pattern_A1 pattern_A1;
    public Pattern_A2 pattern_A2;
    public Pattern_A3 pattern_A3;
    public Pattern_B1 pattern_B1;
    public Pattern_B2 pattern_B2;
    public Pattern_B3 pattern_B3;
    public List<GameObject> phase1Enemies = new List<GameObject>();
    public List<int> phase1Counts = new List<int>();
    public List<float> spawn1CoolTime = new List<float>();

    public List<GameObject> phase2Enemies = new List<GameObject>();
    public List<int> phase2Counts = new List<int>();
    public List<float> spawn2CoolTime = new List<float>();

    public GameObject phase3Enemy;
    public float spawn3CoolTime;

    public List<Transform> spawnPoints;

    public float delayBetweenSpawnPhase;
    
    public float localScaleUp;

    [Inject] public WisuSkillPool skillPool;
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

    private bState lastExecutedPattern = bState.Idle;
    private Transform lookatTransform;
    #endregion

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

        target = currentCharacterProvider.CurrentCharacter.transform;
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
        currentHpValue = Mathf.Lerp(currentHpValue, fullHP, Time.deltaTime * lerpSpeed);
        bossHPSlider.value = currentHpValue;



        //���� �̻� ü�� ���̸� �ó׸ӽ� ���
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
            //���� ���� ����
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

    #region ���� ����
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

    #region ���� ����

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

    #region ���� ���� ǥ��
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

        // ������ ����� ������ ������ ���� ��� ����
        List<bState> availablePatterns = new List<bState>(randomPatterns);
        availablePatterns.Remove(lastExecutedPattern);

        // �������� ���ο� ���� ����
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

        if (imageIndex == 0) // index�� 0�̸� ��� �̹����� ��Ȱ��ȭ
        {
            HideAllImages();
            return;
        }

        // ����Ʈ ���� Ȯ�� �� Ư�� �̹��� Ȱ��ȭ
        if (imageIndex > 0 && imageIndex <= mainstate.bossPatternImages.Count)
        {
            // ���ο� �̹��� Ȱ��ȭ
            currentImageIndex = imageIndex - 1;
            mainstate.bossPatternImages[currentImageIndex].gameObject.SetActive(true);

            // ���� �ð� �� �ش� �̹����� ��Ȱ��ȭ
            StartCoroutine(HideImageAfterDuration(mainstate.displayDuration));
        }
    }

    private IEnumerator HideImageAfterDuration(float duration)
    {
        // ������ �ð���ŭ ���
        yield return new WaitForSeconds(duration);

        // ��� �̹����� ��Ȱ��ȭ
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

    public override void ApplyDamage(DamageInfo damageMessage)
    {
        if (fullHP <= 0 || phaseHPs[0] <= 0.5f)
        {
            return;
        }
        if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || damageMessage.Damager == gameObject || isDead || isInvincible)
        {
            return;
        }

        fullHP--;
        phaseHPs[phaseHPs.Count-1] -= damageMessage.Amount;
        lastDamagedTime = Time.time;

        if (fullHP <= 0 || phaseHPs[0] <= 0.5f)
        {
            ClearBossHPBar();
            return;
        }

        if (hpBarCount > 0 && damageMessage.Amount != 0)
        {
            if (calliSystem != null)
            {
                if (!stackUI.activeSelf) stackUI.SetActive(true);
                calliSystem.Painting(damageMessage.Color, damageMessage.InkStack);

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
    }

    public override void Execution()
    {
        if (paintOverMax && !isDead && isGroggy)
        {
            ClearHPBar();
        }
    }

    public override void DieAction()
    {
        StopAllCoroutines();
        animator.SetBool("GroggyTime", false);
        animator.SetTrigger("Die");

        if (isDead)
        {
            return;
        }
        isDead = true;

        phaseHPs.Clear();
        stackUI.SetActive(false);

        if (KillTrigger != null)
        {
            KillTrigger.OnEnemyKilled(this);
        }

        //대원_TODO: 보스 죽이면 보스 잡았다는 UI 등장
    }
}

[System.Serializable]
public class MainState
{
    public float groggyTime;
    public float phase2Rate;
    public float phase3Rate;
    public float coolTime;
    public List<Image> bossPatternImages;
    public float displayDuration;
    public float phase;
}

[System.Serializable]
public class Pattern_A1
{
    public List<Transform> A1_points;
    public List<float> A1_intervals;
    public float A1_defaultInterval;
    public float A1_waitingTime;
}

[System.Serializable]
public class Pattern_A2
{
    public List<Transform> A2_points;
    public List<float> A2_intervals;
    public float A2_defaultInterval;
    public float A2_waitingTime;
}

[System.Serializable]
public class Pattern_A3
{
    public List<Transform> A3_area1;
    public float A3_area1_Interval;
    public List<Transform> A3_area2;
    public float A3_area2_Interval;
    public List<Transform> A3_area3;
    public float A3_area3_Interval;
    public List<Transform> A3_area4;
    public float A3_area4_Interval;
    public List<Transform> A3_area5;
    public float A3_area5_Interval;
    public float A3_waitingTime;

}

[System.Serializable]
public class Pattern_B1
{
    public List<Transform> B1_area1;
    public float B1_area1_Interval;
    public List<Transform> B1_area2;
    public float B1_area2_Interval;
    public List<Transform> B1_area3;
    public float B1_area3_Interval;
    public List<Transform> B1_area4;
    public float B1_area4_Interval;
    public List<Transform> B1_area5;
    public float B1_area5_Interval;
    public float B1_waitingTime;
}

[System.Serializable]
public class Pattern_B2
{
    public List<Transform> B2_area1;
    public float B2_area1_Interval;
    public List<Transform> B2_area2;
    public float B2_area2_Interval;
    public List<Transform> B2_area3;
    public float B2_area3_Interval;
    public List<Transform> B2_area4;
    public float B2_area4_Interval;
    public List<Transform> B2_area5;
    public float B2_area5_Interval;
    public List<Transform> B2_area6;
    public float B2_area6_Interval;
    public List<Transform> B2_area7;
    public float B2_area7_Interval;
    public List<Transform> B2_area8;
    public float B2_area8_Interval;
    public float B2_waitingTime;
}

[System.Serializable]
public class Pattern_B3
{
    public List<Transform> B3_area1;
    public float B3_area1_Interval;
    public List<Transform> B3_area2;
    public float B3_area2_Interval;
    public List<Transform> B3_area3;
    public float B3_area3_Interval;
    public List<Transform> B3_area4;
    public float B3_area4_Interval;
    public List<Transform> B3_area5;
    public float B3_area5_Interval;
    public float B3_waitingTime;
    public List<Transform> B3_area6;
    public float B3_area6_Interval;
    public List<Transform> B3_area7;
    public float B3_area7_Interval;
}
}