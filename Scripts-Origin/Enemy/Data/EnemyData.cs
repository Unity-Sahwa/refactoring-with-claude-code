using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyData : ScriptableObject
{
    [Header("체력")]
    [Range(1f, 200f)] public float f_hp;

    [Header("체력 바 갯수")]
    [Range(1, 10)] public int i_hpBarCount;

    [Header("추적 속도")]
    [Range(1f, 1000f)] public float f_trackingSpeed;

    [Header("순찰 속도")]
    [Range(1f, 1000f)] public float f_patrolSpeed;

    [Header("순찰 최소 대기 시간")]
    [Range(1, 10)] public int i_patrolWaitingTimeMin;

    [Header("순찰 최대 대기 시간")]
    [Range(1, 10)] public int i_patrolWaitingTimeMax;

    [Header("시야 각")]
    [Range(1f, 359f)] public float f_viewAngle;

    [Header("시야 범위")]
    [Range(1f, 100f)] public float f_viewDistance;

    [Header("몬스터 시체 남아있는 시간(s)")]
    [Range(1f, 60f)] public float f_leftDeadBody;

    [Header("애니메이션 동작 속도")]
    [Range(0f, 3f)] public float f_motionSpeed;
}