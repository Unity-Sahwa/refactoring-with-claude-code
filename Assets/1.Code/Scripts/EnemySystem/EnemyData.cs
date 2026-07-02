using UnityEngine;

namespace Refactoring
{
    public class EnemyData : ScriptableObject
    {
        [Range(1f, 200f)] public float f_hp;
        [Range(1, 10)] public int i_hpBarCount;
        [Range(1f, 1000f)] public float f_trackingSpeed;
        [Range(1f, 1000f)] public float f_patrolSpeed;
        [Range(1, 10)] public int i_patrolWaitingTimeMin;
        [Range(1, 10)] public int i_patrolWaitingTimeMax;
        [Range(1f, 359f)] public float f_viewAngle;
        [Range(1f, 100f)] public float f_viewDistance;
        [Range(1f, 60f)] public float f_leftDeadBody;
        [Range(0f, 3f)] public float f_motionSpeed;
    }
}
