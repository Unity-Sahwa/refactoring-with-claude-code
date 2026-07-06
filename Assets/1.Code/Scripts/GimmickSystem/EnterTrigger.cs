using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class EnterTrigger : MonoBehaviour
    {
        public enum TriggerTarget
        {
            Player,
            Enemy
        }

        public MeshRenderer meshRenderer;
        public TriggerTarget triggerTarget = TriggerTarget.Player;
        public List<EventData> enterEvents;
        public List<float> delayTimes;
        public bool isLoop = false;

        private bool hasTriggered = false;

        private void Start()
        {
            meshRenderer.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && !isLoop)
            {
                return;
            }
            if ((triggerTarget == TriggerTarget.Player && other.gameObject.layer == LayerMask.NameToLayer("Player")) ||
                (triggerTarget == TriggerTarget.Enemy && other.CompareTag("Enemy")))
            {
        
                while (delayTimes.Count < enterEvents.Count)
                {
                    delayTimes.Add(0f);
                }
                for (int i = 0; i < enterEvents.Count; i++)
                {
                    if (enterEvents[i] != null)
                    {
                        StartCoroutine(ExecuteEventWithDelay(enterEvents[i], delayTimes[i]));
                    }
                }
                hasTriggered = true;
            }
        }
        private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
        {
            yield return new WaitForSeconds(delay);
            eventData.Execute();
        }
    }  
}

