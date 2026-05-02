using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObjectActiveFalse : EventData
{
    public override void Execute()
    {
        gameObject.SetActive(false);
    }
}
