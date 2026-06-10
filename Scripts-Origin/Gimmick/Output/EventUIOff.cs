using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventUIOff : EventData
{
    public override void Execute()
    {
        gameObject.SetActive(false);
    }
}
