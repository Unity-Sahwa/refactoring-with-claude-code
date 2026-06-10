using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventUIOn : EventData
{
    public override void Execute()
    {
        gameObject.SetActive(true);
    }
}