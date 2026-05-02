using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObjectRemove : EventData
{
    [Header("삭제되는 시간(s)")]
    [Range(1, 10)] public int deleteTime = 1;

    public override void Execute()
    {
        Destroy(gameObject, deleteTime);
    }
}
