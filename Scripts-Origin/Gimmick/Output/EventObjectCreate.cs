using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObjectCreate : EventData
{
    [Header("생성할 오브젝트 프리팹")]
    public GameObject objectPrefab;

    [Header("생성 위치")]
    public Transform spawnPoint;

    public override void Execute()
    {
        if (objectPrefab != null && spawnPoint != null)
        {
            Instantiate(objectPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
