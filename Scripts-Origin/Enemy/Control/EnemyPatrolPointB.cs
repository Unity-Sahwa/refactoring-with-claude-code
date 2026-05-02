using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolPointB : MonoBehaviour
{
    public float radius = .5f;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
