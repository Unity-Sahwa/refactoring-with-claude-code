using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventVisualEffectOn : EventData
{
    public override void Execute()
    {
        gameObject.SetActive(true);
        var particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }
}
