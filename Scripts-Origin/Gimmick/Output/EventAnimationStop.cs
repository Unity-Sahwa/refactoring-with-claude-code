using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAnimationStop : EventData
{
    private Animator targetAnimator;

    private void Awake()
    {
        targetAnimator = GetComponent<Animator>();
    }

    public override void Execute()
    {
        if (targetAnimator != null)
        {
            targetAnimator.speed = 0;
        }
    }
}
