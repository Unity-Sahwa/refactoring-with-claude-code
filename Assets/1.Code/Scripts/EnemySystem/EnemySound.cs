using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Refactoring
{
public class EnemySound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;
    public AudioMixerGroup mixerGroup;
    public float minDistance = 1f;
    public float maxDistance = 40f;
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
}