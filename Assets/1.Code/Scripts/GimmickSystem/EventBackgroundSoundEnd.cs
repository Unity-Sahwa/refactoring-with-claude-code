using UnityEngine;

namespace Refactoring
{
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
}

