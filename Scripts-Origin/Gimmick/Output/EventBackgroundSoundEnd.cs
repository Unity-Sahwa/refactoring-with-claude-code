using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBackgroundSoundEnd : EventData
{
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override void Execute()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
