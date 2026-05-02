using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTeleport : EventData
{
    [Header("순간이동할 타겟 태그")]
    public string targetTag = "Player";

    [Header("순간이동할 위치")]
    public GameObject teleportTarget;

    [Header("검색 범위")]
    public float radius = 10;

    private MeshRenderer meshRenderer;

    public override void Execute()
        {
            var patrolColliders = Physics.OverlapSphere(transform.position, radius);
            foreach (var collider in patrolColliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    if (meshRenderer != null && meshRenderer.enabled == true)
                    {
                        meshRenderer = teleportTarget.GetComponent<MeshRenderer>();
                        meshRenderer.enabled = false;
                    }

                    collider.transform.position = teleportTarget.transform.position;
                }
            }
        }
        private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}