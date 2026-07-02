using UnityEngine;

namespace Refactoring
{
public class EnemyPatrolPointA : MonoBehaviour
{
    public float radius = .5f;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
}