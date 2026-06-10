using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolPointA : MonoBehaviour
{
    public float radius = .5f;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
