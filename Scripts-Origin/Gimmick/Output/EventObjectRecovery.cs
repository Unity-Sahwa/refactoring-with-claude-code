using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObjectRecovery : EventData
{
    [Header("제어할 오브젝트들")]
    public List<ObjectInitialize> objectInitSavers;

    public override void Execute()
    {
        if (objectInitSavers != null && objectInitSavers.Count > 0)
        {
            foreach (var saver in objectInitSavers)
            {
                if (saver != null)
                {
                    saver.RestoreInitialState();
                }
            }
        }
    }
}
