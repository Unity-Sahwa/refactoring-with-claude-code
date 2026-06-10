using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeForSwitchSceneTrigger : EventData
{
    [SerializeField] private float startTimeRate =0 ;
    [SerializeField] private float endTimeRate = 0;

    public override void Execute()
    {
        LoadingUI.instance.FadeOutInScreen(startTimeRate, endTimeRate);
    }
}
