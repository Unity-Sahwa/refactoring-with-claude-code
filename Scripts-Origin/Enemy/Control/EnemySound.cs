using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnemySound : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("클립 넣는 곳")]
    public AudioClip[] audioClips;


    public AudioMixerGroup mixerGroup;

    [Header("사운드가 최대로 들리는 거리")]
    public float minDistance = 1f;

    [Header("사운드가 아예 안들리는 거리")]
    public float maxDistance = 40f;

    [Header("사운드 쿨타임")]
    public float soundCooldown = 5f;

    private bool canPlay = true;

    private void Start()
    {
        audioSource.outputAudioMixerGroup = mixerGroup;

        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
    }


    public void PlaySoundEffect(int num)
    {
        if (num != 0)
        {
            audioSource.PlayOneShot(audioClips[num-1]);
            StartCoroutine(SoundCooldownRoutine());
        }
        if (canPlay && audioClips.Length > 0)
        {   
            var randomIndex = Random.Range(0, audioClips.Length);
            audioSource.PlayOneShot(audioClips[randomIndex]);
            StartCoroutine(SoundCooldownRoutine());
        }
    }

    IEnumerator SoundCooldownRoutine()
    {
        canPlay = false;
        yield return new WaitForSeconds(soundCooldown); 
        canPlay = true;
    }
}
