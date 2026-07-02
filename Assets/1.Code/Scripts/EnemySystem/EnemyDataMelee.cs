using UnityEngine;

namespace Refactoring
{
[CreateAssetMenu(fileName = "EnemyDataMelee", menuName = "Data/EnemyDataMelee")]
public class EnemyDataMelee : EnemyData
{
    [Range(1f, 100f)] public float f_autoAttackMeleeDamage;
    [Range(1f, 10f)] public float f_autoAttackMeleeMotionCoolTime;
    [Range(1f, 10f)] public float f_autoAttackMeleeRange;
}
}
