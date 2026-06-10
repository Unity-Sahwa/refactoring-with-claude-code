using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WisuStart : EventData
{
    public WisuMainRe wisuMain;
    public override void Execute()
    {
        if (wisuMain != null)
        {
            wisuMain.animator.SetTrigger("FirstPhaseStart");
        }
    }    
}
