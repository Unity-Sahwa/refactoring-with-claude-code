using UnityEngine;

namespace Refactoring
{
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
}
