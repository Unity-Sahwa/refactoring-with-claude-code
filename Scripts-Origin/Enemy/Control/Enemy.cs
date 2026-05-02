using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable
{
    protected enum eState
    {
        A,
        Patrol,
        Tracking,
        AttackBegin,
        Attacking,
        Hit,
        Idle
    }
    protected eState enemyState;

    [Header("트리거 몬스터")]
    public bool isKillTrigger = false;

    [Header("죽였을 떄 실행할 이벤트")]
    public List<EventData> DeathEvent;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    [Header("킬 트리거 넣어줄 곳")]
    public KillTrigger KillTrigger;

    [Header("순찰 지점 A 넣는 곳")]
    public Transform patrolPointA;

    [Header("순찰 지점 B 넣는 곳")]
    public Transform patrolPointB;


    [Header("변수들")]
    public Transform attackRoot;
    public Transform viewTransform;
    public EnemySound audioSourceIdle;
    public EnemySound audioSourceAttack;
    public EnemySound audioSourceFindTarget;
    public EnemySound audioSourceHit;
    public EnemySound audioSourceDie;

    public event Action OnDeath; // 적이 죽을 때 발생하는 이벤트  
    protected LayerMask targetLayer;
    protected NavMeshAgent navAgent;
    [HideInInspector] public Player target;
    protected Rigidbody rigidbody;
    protected List<Player> lastAttackedTarget = new List<Player>();
    public Animator animator;
    private Coroutine hitStopCoroutine;

    //공격 사거리
    protected float attackDistance;

    //몸통 회전 변수
    protected float turnSmoothTime = 0.1f;
    protected float turnSmoothVelocity = 0.1f;

    //피격 무적 시간
    protected const float MIN_TIME_BET_DAMAGE = 0.1f;

    //시야에서 플레이어를 놓쳤을 때 타이머
    protected float lostSightTime = 10.0f;
    protected float lostSightTimer = 0.0f;

    //최근 (적 개체의)공격 시각
    protected float lastAttackTime = 0;

    //최근 (플레이어로 부터)데미지 당한 시각
    protected float lastDamagedTime = 0;

    protected bool patrolToB;
    protected bool isWaiting;
    protected bool isAttack;

    //대기 시간 변수
    protected float waitTime;
    //체력 변수
    protected List<float> phaseHPs;

    [HideInInspector] public bool isDead = false;

    //상태 추적 변수
    protected eState previousState;

    #region Scriptable Object Data
    public float hp;
    public float hpBarCount;
    public float phaseHP;
    public float knockbackForce = 1000f;
    public float knockbackDuration = 0.5f;

    [HideInInspector] public float trackingSpeed;
    [HideInInspector] public float patrolSpeed;
    [HideInInspector] public float patrolWaitingTimeMin;
    [HideInInspector] public float patrolWaitingTimeMax;
    [HideInInspector] public float viewAngle;
    [HideInInspector] public float viewDistance;
    [HideInInspector] public float autoAttackDamage;
    [HideInInspector] public float autoAttackMotionCoolTime;
    [HideInInspector] public float autoAttackRange;
    [HideInInspector] public float leftDeadBody;
    #endregion

    #region 덧칠 시스템
    protected CalliSystem calliSystem;
    public GameObject[] paintOverStacks;
    public GameObject stackUI;
    protected bool paintOverMax;
    #endregion

    protected bool hasTarget => target != null;

    protected virtual void Awake()
    {
        targetLayer = LayerMask.GetMask("Player");
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        calliSystem = GetComponent<CalliSystem>();
        rigidbody = GetComponent<Rigidbody>();
        enemyState = eState.Idle;
        previousState = eState.A;

        for (int i = 0; i < m_skinnedMeshResnderers.Length; i++)
        {
            Material[] materials = m_skinnedMeshResnderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                // 각 Material을 복사본으로 변경
                materials[j] = new Material(materials[j]);
            }
            m_skinnedMeshResnderers[i].materials = materials;

            // 복사된 Material을 리스트에 추가
            foreach (var mat in materials)
            {
                m_materials.Add(mat);
            }
        }

    }

    protected virtual void Start()
    {
        phaseHPs = new List<float>();
        phaseHP = hp / hpBarCount;
        for (var i = 0; i < hpBarCount; i++)
        {
            phaseHPs.Add(phaseHP);
        }

        var attackPivot = attackRoot.position;
        attackPivot.y = transform.position.y;

        attackDistance = Vector3.Distance(transform.position, attackPivot) + autoAttackRange;
        navAgent.stoppingDistance = attackDistance;
        DissolveReset();
        StartCoroutine(StateManager());
    }

    protected virtual void Update()
    {
        animator.SetFloat("Speed", navAgent.desiredVelocity.magnitude);
        if (isDead)
        {
            return;
        }

        InSightTargetCheck();
    }
    protected virtual void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }
        audioSourceIdle.PlaySoundEffect(0);
        if (hasTarget && enemyState == eState.AttackBegin || enemyState == eState.Attacking)
        {
            var lookRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            var targetAngleY = lookRotation.eulerAngles.y;

            targetAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity, turnSmoothTime);
            transform.eulerAngles = Vector3.up * targetAngleY;
        }
    }

    #region 시야 체크
    public void InSightTargetCheck()
    {
        if (hasTarget)
        {
            if (!IsTargetOnSight(target.transform))
            {
                lostSightTimer += Time.deltaTime;
                if (lostSightTimer >= lostSightTime)
                {
                    target = null;
                    navAgent.ResetPath();
                    enemyState = eState.Idle;
                }
            }
            else
            {
                lostSightTimer = 0f;
            }
        }
        else
        {
            var colliders = Physics.OverlapSphere(viewTransform.position, viewDistance, targetLayer);

            foreach (var collider in colliders)
            {
                if (!IsTargetOnSight(collider.transform))
                {
                    continue;
                }
                target = collider.GetComponent<Player>();

                enemyState = eState.Tracking;
                lostSightTimer = 0;
                audioSourceFindTarget.PlaySoundEffect(0);
                patrolPointA = null;
                patrolPointB = null;
            }
        }
    }

    protected bool IsTargetOnSight(Transform target)
    {
        var direction = target.position - viewTransform.position;
        direction.y = viewTransform.forward.y;
        if (Vector3.Angle(direction, viewTransform.forward) > viewAngle * 0.5f)
        {
            return false;
        }
        direction = target.position - viewTransform.position;

        RaycastHit hit;

        if (Physics.Raycast(viewTransform.position, direction, out hit, viewDistance, targetLayer))
        {
            if (hit.transform == target)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region 데미지 받을때
    public virtual bool ApplyDamage(DamageMessage damageMessage)
    {
        if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || damageMessage.damager == gameObject || isDead)
        {
            return false;
        }

        phaseHPs[phaseHPs.Count - 1] -= damageMessage.amount;

        lastDamagedTime = Time.time;

        enemyState = eState.Hit;
        animator.SetTrigger("Hit");
        audioSourceHit.PlaySoundEffect(0);

        if (hitStopCoroutine != null)
        {
            StopCoroutine(hitStopCoroutine);
        }
       
        if (target == null)
        {
            target = damageMessage.damager.GetComponent<Player>();
        }

        //대원 임시수정: 플레이어 앞쪽으로 모이도록

        hitStopCoroutine = StartCoroutine(HitStop(2f, target.transform, knockbackForce));

        if (phaseHPs[phaseHPs.Count - 1] <= 0)
        {
            ClearHPBar();
        }

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

    #region 상태 관리
    public IEnumerator StateManager()
    {
        while (!isDead)
        {
            switch (enemyState)
            {
                case eState.Idle:
                    IdleLogic();
                    break;
                case eState.Patrol:
                    PatrolLogic();
                    break;
                case eState.Tracking:
                    TrackingLogic();
                    break;
                case eState.AttackBegin:
                    AttackLogic();
                    break;
            }
            yield return null;
        }
    }
    #endregion

    #region 기본 상태
    public virtual void IdleLogic()
    {
        if (navAgent.speed != patrolSpeed)
        {
            navAgent.speed = patrolSpeed;
        }
        if (animator.GetBool("BattleMode"))
        {
            animator.SetBool("BattleMode", false);
        }
        if (isWaiting)
        {
            StartCoroutine(WaitBeforePatrol(2f));
        }
        if (patrolPointA != null && !isWaiting)
        {
            enemyState = eState.Patrol;
        }
    }
    #endregion

    #region 순찰 상태
    public void PatrolLogic()
    {
        if (patrolToB && patrolPointB != null)
        {
            var destination = new Vector3(patrolPointB.position.x, transform.position.y, patrolPointB.position.z);
            navAgent.SetDestination(destination);
            if (navAgent.remainingDistance <= navAgent.stoppingDistance && !navAgent.pathPending)
            {
                patrolToB = false;
                isWaiting = true;
                enemyState = eState.Idle;
            }
        }
        else
        {
            var destination = new Vector3(patrolPointA.position.x, transform.position.y, patrolPointA.position.z);

            if (navAgent != null && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(destination);
            }
            if (navAgent.remainingDistance <= navAgent.stoppingDistance && !navAgent.pathPending)
            {
                patrolToB = true;
                isWaiting = true;
                enemyState = eState.Idle;
            }
        }
    }
    #endregion 

    #region 추적 상태
    public void TrackingLogic()
    {
        if (navAgent.speed != trackingSpeed)
        {
            navAgent.speed = trackingSpeed;
        }
        if (!animator.GetBool("BattleMode"))
        {
            animator.SetBool("BattleMode", true);
        }
        if (hasTarget)
        {
            var destination = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            navAgent.SetDestination(destination);
            //Vector3 TargetPosition = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            //var distance = Vector3.Distance(TargetPosition, transform.position);
            //if (distance <= attackDistance)
            if (navAgent.remainingDistance <= attackDistance && !navAgent.pathPending)
            {
                if (Time.time > lastAttackTime + autoAttackMotionCoolTime)
                {
                    enemyState = eState.AttackBegin;
                }
            }
        }
    }
    #endregion

    #region 전투 상태
    protected virtual void AttackLogic()
    {

    }
    #endregion

    #region 공격 애니메이션 지점 설정
    protected virtual void EnableAttack()
    {

    }
    #endregion

    #region 공격 끝
    protected virtual void DisableAttack()
    {

    }
    #endregion

    #region 사망처리
    public virtual void DieAction()
    {
        StopAllCoroutines();
        animator.SetTrigger("Die");

        if (isDead)
        {
            return;
        }
        if (navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
        }
        rigidbody.isKinematic = true;
        isDead = true;
        audioSourceDie.PlaySoundEffect(0);
        phaseHPs.Clear();
        stackUI.SetActive(false);

        GetComponent<Collider>().enabled = false;

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

        StartCoroutine(DeactivateAfterDelay());
    } 

    private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventData.Execute();
    }

    protected IEnumerator DeactivateAfterDelay()
    {
        var timer = 0f;

        while (timer < leftDeadBody)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
    #endregion

    #region 처형 당할때
    public virtual void Execution()
    {
        if (!isDead)
        {
            MotionStop(5f);
            //todo 처형 당하는 모션과 사운드
            ClearHPBar();
        }
    }
    #endregion

    #region 체력 바 감소
    public void ClearHPBar()
    {
        phaseHPs.RemoveAt(phaseHPs.Count - 1);
        hpBarCount--;
        if (hpBarCount <= 0)
        {
            DieAction();
        }
    }

    public void ClearBossHPBar()
    {
        DieAction();
        return;
    }

    #endregion

    #region 모션 정지
    public void MotionStop(float waitTime)
    {
        if (waitTime < 0.1)
        {
            StartCoroutine(StateManager());
        }
        else
        {
            StopAllCoroutines();

            if (navAgent != null && navAgent.enabled)
            {
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
            }

            StartCoroutine(WaitBeforeMotion(waitTime));
            StartCoroutine(StateManager());
        }
    }
    #endregion

    #region 코루틴
    protected IEnumerator WaitBeforePatrol(float waitTime)
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
    }

    protected IEnumerator WaitBeforeMotion(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        navAgent.isStopped = false;
    }

    protected IEnumerator WaitBeforeTracking(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        //대원 임시수정: 몬스터의 네브메쉬 컴포넌트가 비활성화된 상태에서 이 함수 진입하는 경우가 있음
        this.GetComponent<NavMeshAgent>().enabled = true;

        navAgent.isStopped = false;
        lastAttackTime = Time.time;
    }

    protected IEnumerator HitStop(float waitTime, Transform target, float knockbackForce)
    {
        //대원 임시수정: 몬스터의 네브메쉬 컴포넌트가 비활성화된 상태에서 이 함수 진입하는 경우가 있음
        this.GetComponent<NavMeshAgent>().enabled = true;
        
        // NavMeshAgent를 멈추고, 이동 속도 0으로
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;

        // 만약 넉백 대상과 넉백 힘이 유효하면, 넉백 로직 실행
        if (target != null && knockbackForce > 0f)
        {
            navAgent.enabled = false;
            rigidbody.isKinematic = false;

            Vector3 knockbackDirection = (transform.position - target.position).normalized;

            // 원하는 ForceMode에 따라 충돌 느낌 다름
            rigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(waitTime);

        if (target != null && knockbackForce > 0f)
        {
            rigidbody.isKinematic = true;
            navAgent.enabled = true;
        }

        navAgent.isStopped = false;

        enemyState = eState.Tracking;
    }

    //protected IEnumerator HitStop(float waitTime)
    //{
    //    navAgent.isStopped = true;
    //    navAgent.velocity = Vector3.zero;
    //    yield return new WaitForSeconds(waitTime);
    //    enemyState = eState.Tracking;
    //    navAgent.isStopped = false;
    //}
    #endregion

    #region 디졸브
    [Header("디졸브 관련")]
    public float DissolveSpeed = 0.01f;
    public float DissolveYield = 0.1f;

    public ParticleSystem Particle = null;

    private const string DISSOVE_AMOUNT = "_DissolveAmount";

    public SkinnedMeshRenderer[] m_skinnedMeshResnderers;
    public List<Material> m_materials = new List<Material>();

    private float m_dissolveStart = -0.2f;
    private float m_dissolveEnd = 1.2f;

    public void Dissolve()
    {
        StartCoroutine(DissolveCoroutine());
    }
    public void DissolveReset()
    {
        foreach (Material matertial in m_materials)
        {
            matertial.SetFloat(DISSOVE_AMOUNT, m_dissolveStart);
        }
    }
    private IEnumerator DissolveCoroutine()
    {
        if (Particle != null)
        {
            Particle.Play();
        }

        if (m_materials.Count > 0)
        {
            float dissovleAmount = m_dissolveStart;
            float speedMulti = 1f;
            while (dissovleAmount < m_dissolveEnd)
            {
                dissovleAmount += DissolveSpeed * speedMulti;
                speedMulti += 0.1f;
                foreach (Material matertial in m_materials)
                {
                    matertial.SetFloat(DISSOVE_AMOUNT, dissovleAmount);
                }
                yield return new WaitForSeconds(DissolveYield);
            }
        }
    }
    #endregion
}
