using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventVisualEffectOff : EventData
{
    public override void Execute()
    {
        var particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
        gameObject.SetActive(false);
    }
}
