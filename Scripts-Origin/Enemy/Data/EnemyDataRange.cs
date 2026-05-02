using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data Range", menuName = "Scriptable Objects/Enemy Data/Range")]
public class EnemyDataRange : EnemyData
{
    [Header("원거리 평타 데미지")]
    [Range(1f, 100f)] public float f_autoAttackRangedDamage;

    [Header("원거리 평타 사이 간격")]
    [Range(1f, 100f)] public float f_autoAttackRangedMotionCoolTime;

    [Header("원거리 평타 공격 범위")]
    [Range(1f, 100f)] public float f_autoAttackRangedRange;

    [Header("투사체 삭제 시간")]
    [Range(1f, 10f)] public float f_deleteTime;

    [Header("투사체 속도")]
    [Range(1f, 10f)] public float f_projectileSpeed;
}
