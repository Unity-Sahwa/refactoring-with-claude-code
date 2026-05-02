using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBackgroundSoundStart : EventData
{
    private AudioSource audioSource;
    private AudioClip audioClip;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override void Execute()
    {
        if (audioSource != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }
}
