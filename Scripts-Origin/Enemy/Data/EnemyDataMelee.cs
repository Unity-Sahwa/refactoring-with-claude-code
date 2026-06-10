using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data Melee", menuName = "Scriptable Objects/Enemy Data/Melee")]
public class EnemyDataMelee : EnemyData
{
    [Header("근접 평타 데미지")]
    [Range(1f, 100f)] public float f_autoAttackMeleeDamage;

    [Header("근접 평타 사이 간격")]
    [Range(1f, 10f)] public float f_autoAttackMeleeMotionCoolTime;

    [Header("근접 평타 공격 범위")]
    [Range(1f, 10f)] public float f_autoAttackMeleeRange;
}
