using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventEffectSound : EventData
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
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}
