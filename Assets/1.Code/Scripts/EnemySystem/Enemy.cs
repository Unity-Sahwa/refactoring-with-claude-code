using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Refactoring
{
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

        protected LayerMask targetLayer;
        protected NavMeshAgent navAgent;
        [HideInInspector] public Transform target;
        protected Rigidbody rb;
        protected List<IDamageable> lastAttackedTarget = new List<IDamageable>();
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
        public float knockbackForce = 2000f;
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
        protected CaliSystem calliSystem;
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
            calliSystem = GetComponent<CaliSystem>();
            rb = GetComponent<Rigidbody>();
            enemyState = eState.Idle;
            previousState = eState.A;
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
            if (hasTarget && (enemyState == eState.AttackBegin || enemyState == eState.Attacking))
            {
                var lookRotation = Quaternion.LookRotation(target.gameObject.transform.position - transform.position);
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
                    
                    if (collider.GetComponent<IDamageable>() == null) 
                    {
                        continue;
                    }
                    target = collider.transform;

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
            Vector3 aim = target.position + Vector3.up * 0.9f;
            direction = aim - viewTransform.position;

            RaycastHit hit;

            if (Physics.SphereCast(viewTransform.position, 0.4f, direction, out hit, viewDistance, targetLayer))
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
        public virtual void ApplyDamage(DamageInfo damageMessage)
        {
            if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || damageMessage.Damager == gameObject || isDead)
            {
                return;
            }

            phaseHPs[phaseHPs.Count - 1] -= damageMessage.Amount;

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
                target = damageMessage.Damager.transform;
            }

            hitStopCoroutine = StartCoroutine(HitStop(target.position, 2f));

            if (phaseHPs[phaseHPs.Count - 1] <= 0)
            {
                ClearHPBar();
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
                            paintOverStacks[i].GetComponent<Image>().color = Color.red;
                        }
                        paintOverMax = true;
                    }
                }
            }
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
            if (hasTarget && navAgent.isOnNavMesh)
            {
                var destination = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
                navAgent.SetDestination(destination);
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

        // 전투 상태
        protected virtual void AttackLogic() { }

        // 공격 애니메이션 지점 설정
        protected virtual void EnableAttack() { }

        // 공격 끝
        protected virtual void DisableAttack() { }

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
            rb.isKinematic = true;
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
            this.GetComponent<NavMeshAgent>().enabled = true;

            navAgent.isStopped = false;
            lastAttackTime = Time.time;
        }

        // 피격 경직: 넉백으로 밀린 뒤 잠깐 멈췄다가 다시 추적 상태로 복귀함.
        // 넉백 자체는 Knockback 코루틴이 담당함(여기선 호출만).
        protected IEnumerator HitStop(Vector3 attackerPos, float stunTime)
        {
            // 먼저 넉백으로 밀림. 끝날 때까지 기다림.
            yield return Knockback(attackerPos);

            // 밀린 뒤 남은 시간동안 멈춰서 경직됨.
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
            yield return new WaitForSeconds(stunTime);
            navAgent.isStopped = false;

            enemyState = eState.Tracking;
        }

        // 넉백: 맞은 반대 방향으로 밀어냄. Rigidbody 물리(AddForce) 대신 NavMeshAgent.Move로 밀어줌.
        // agent 켠 채로 밀어서 몬스터끼리 avoidance가 겹침을 자동으로 정리함(뭉쳐도 안 튐, navmesh 밖으로도 안 나감).
        protected IEnumerator Knockback(Vector3 attackerPos)
        {
            navAgent.enabled = true;
            navAgent.ResetPath();       // 추적하던 목적지 지움. 안 지우면 agent가 밀린 걸 도로 끌고 감.
            navAgent.isStopped = false; // isStopped=true면 Move가 씹혀서 안 밀림. 반드시 풀어야 함.

            Vector3 dir = (transform.position - attackerPos).normalized;
            dir.y = 0f;

            // knockbackForce를 초기 속도(m/s)로 씀. 예전 Impulse 값과 단위 달라서 인스펙터에서 재조정 필요함.
            // ponytail: 감쇠는 선형(Lerp). 곡선 넉백 필요하면 그때 곡선으로 바꾸기.
            float duration = Mathf.Max(knockbackDuration, 0.05f); // 0이면 아예 안 밀리니 최소값 보장.
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float speed = Mathf.Lerp(knockbackForce, 0f, elapsed / duration);
                navAgent.Move(dir * speed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        #endregion

        protected virtual void OnDrawGizmos()
        {
            if (viewTransform == null) return;

            Gizmos.color = hasTarget ? Color.red : Color.green;   // 발견 시 빨강, 아니면 초록
            Vector3 origin = viewTransform.position;

            // 감지 범위(OverlapSphere) = viewDistance 반경 구체
            Gizmos.DrawWireSphere(origin, viewDistance);

            // 시야각 경계선
            Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * viewTransform.forward;
            Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * viewTransform.forward;
            Gizmos.DrawLine(origin, origin + left * viewDistance);
            Gizmos.DrawLine(origin, origin + right * viewDistance);

            // 타깃 있을 때 SphereCast 프로브(반지름 0.4, 몸통 조준) 표시
            if (target != null)
            {
                Vector3 aim = target.position + Vector3.up * 0.9f;
                Gizmos.DrawWireSphere(aim, 0.4f);
            }
        }
    }
}
