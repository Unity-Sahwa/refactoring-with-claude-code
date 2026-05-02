using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEditor;

public class EnemyRange : Enemy
{
    public EnemyDataRange enemyDataRange;
    public GameObject waterBallPrefabs;
    [HideInInspector] public GameObject WaterBallObject;
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public float distanceToPlayerMax; // 플레이어와의 최대 거리(참조 없음)
    [HideInInspector] public float projectileSpeed; // 물체 속력

    #region Start()
    protected override void Start()
    {
        hp = enemyDataRange.f_hp;
        hpBarCount = enemyDataRange.i_hpBarCount;
        trackingSpeed = enemyDataRange.f_trackingSpeed;
        patrolSpeed = enemyDataRange.f_patrolSpeed;
        patrolWaitingTimeMin = enemyDataRange.i_patrolWaitingTimeMin;
        patrolWaitingTimeMax = enemyDataRange.i_patrolWaitingTimeMax;
        viewAngle = enemyDataRange.f_viewAngle;
        viewDistance = enemyDataRange.f_viewDistance;
        autoAttackDamage = enemyDataRange.f_autoAttackRangedDamage;
        autoAttackMotionCoolTime = enemyDataRange.f_autoAttackRangedMotionCoolTime;
        autoAttackRange = enemyDataRange.f_autoAttackRangedRange;
        leftDeadBody = enemyDataRange.f_leftDeadBody;

        projectileSpeed = enemyDataRange.f_projectileSpeed;
        base.Start();
    }
    #endregion


    #region 전투 상태
    protected override void AttackLogic()
    {
        if (!isAttack)
        {
            if (hasTarget)
            {
                isAttack = true;
                audioSourceAttack.PlaySoundEffect(0);
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
    #endregion

    #region 공격 애니메이션 지점 설정
    protected override void EnableAttack()
    {
        enemyState = eState.Attacking;
        ThrowWaterBall();
    }
    #endregion

    #region 공격 끝
    protected override void DisableAttack()
    {
        enemyState = eState.Tracking;
        waitTime = 1f;
        isAttack= false;    
        StartCoroutine(WaitBeforeTracking(waitTime));
    }
    #endregion

    #region 마법 시전
    private void ThrowWaterBall()
    {
        if (hasTarget)
        {
            Vector3 targetPosition = target.transform.position;
            targetPosition.y = attackRoot.position.y;

            direction = (targetPosition - attackRoot.position).normalized;
        }
        else
        {
            direction = attackRoot.forward;
        }

        WaterBallObject = Instantiate(waterBallPrefabs, attackRoot.position, Quaternion.LookRotation(direction));
        var WaterBallScript = waterBallPrefabs.GetComponent<WaterBall>();

        if (WaterBallScript != null)
        {
            WaterBallScript.damage = autoAttackDamage;
        }

        Rigidbody rigidbody = WaterBallObject.GetComponent<Rigidbody>();
        rigidbody.velocity = direction * projectileSpeed;
        lastAttackTime = Time.time;
    }
    #endregion

    #region 코루틴
    protected IEnumerator WaitForAttackCooldown()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
    }
    #endregion

}


