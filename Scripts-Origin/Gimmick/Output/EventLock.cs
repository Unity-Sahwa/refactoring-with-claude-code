using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLock : EventData
{
    [Header("Overlapsphere 반경")]
    public float radius = 5f;

    [Header("적용할 레이어(중복 가능)")]
    public LayerMask layerMask;

    // 중지된 객체들을 저장할 리스트
    public List<GameObject> lockedTargets = new List<GameObject>();

    public override void Execute()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);

        foreach (var hitCollider in hitColliders)
        {
            var target = hitCollider.gameObject;
            if (target != null)
            {
                var components = target.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    component.enabled = false;
                }

                var playerController = target.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.enabled = false;
                }

                var enemy = target.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.MotionStop(999f);
                }

                lockedTargets.Add(target);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
