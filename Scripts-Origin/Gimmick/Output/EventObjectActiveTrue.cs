using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObjectActiveTrue : EventData
{
    [Header("활성화 될 위치")]
    public Vector3 relativeSpawnPosition;

    public override void Execute()
    {
        transform.position += relativeSpawnPosition;
        gameObject.SetActive(true);
    }
}
