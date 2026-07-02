using UnityEngine;

namespace Refactoring
{
    public class EnemyMelee : Enemy
    {
        private RaycastHit[] hits = new RaycastHit[10];
        public EnemyDataMelee enemyDataMelee;
        protected override void Start()
        {
            hp = enemyDataMelee.f_hp;
            hpBarCount = enemyDataMelee.i_hpBarCount;
            trackingSpeed = enemyDataMelee.f_trackingSpeed;
            patrolSpeed = enemyDataMelee.f_patrolSpeed;
            patrolWaitingTimeMin = enemyDataMelee.i_patrolWaitingTimeMin;
            patrolWaitingTimeMax = enemyDataMelee.i_patrolWaitingTimeMax;
            viewAngle = enemyDataMelee.f_viewAngle;
            viewDistance = enemyDataMelee.f_viewDistance;
            autoAttackDamage = enemyDataMelee.f_autoAttackMeleeDamage;
            autoAttackMotionCoolTime = enemyDataMelee.f_autoAttackMeleeMotionCoolTime;
            autoAttackRange = enemyDataMelee.f_autoAttackMeleeRange;
            leftDeadBody = enemyDataMelee.f_leftDeadBody;

            base.Start();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (enemyState == eState.Attacking)
            {
                var direction = transform.forward;
                var deltaDistance = navAgent.velocity.magnitude * Time.deltaTime;
                var size = Physics.SphereCastNonAlloc(attackRoot.position, autoAttackRange, direction, hits, deltaDistance, targetLayer);

                for (var i = 0; i < size; i++)
                {
                    var attackTarget = hits[i].collider.GetComponent<IDamageable>();
                    if (attackTarget != null && !lastAttackedTarget.Contains(attackTarget))
                    {
                        var message = new DamageInfo();
                        message.Amount = autoAttackDamage;
                        message.Damager = gameObject;
                        message.HitPoint = hits[i].distance <= 0f ? attackRoot.position : hits[i].point;
                        
                        attackTarget.ApplyDamage(message);
                        lastAttackedTarget.Add(attackTarget);

                        break;
                    }
                }
            }
        }

        protected override void AttackLogic()
        {
            if (!isAttack)
            {
                if (hasTarget)
                {
                    isAttack = true;
                    lastAttackTime = Time.time;
                    navAgent.isStopped = true;
                    animator.SetTrigger("Attack");
                }
                else
                {
                    enemyState = eState.Idle;
                }
            }
            else
            {
                enemyState = eState.Tracking;
                isAttack = false;               
            }
        }
        protected override void EnableAttack()
        {
            audioSourceAttack.PlaySoundEffect(0);
            enemyState = eState.Attacking;
            lastAttackedTarget.Clear();
        }
        protected override void DisableAttack()
        {
            enemyState = eState.Tracking;
            waitTime = 1f;
            isAttack = false;
            StartCoroutine(WaitBeforeTracking(waitTime));
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();   // 부모 시야 기즈모 유지

            if (attackRoot == null) return;
            Gizmos.color = enemyState == eState.Attacking ? Color.red : Color.yellow;  // 공격 중 빨강
            Gizmos.DrawWireSphere(attackRoot.position, autoAttackRange);
        }
    }
}