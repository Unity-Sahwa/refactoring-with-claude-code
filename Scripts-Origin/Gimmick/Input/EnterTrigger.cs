using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnterTrigger : MonoBehaviour
{
    public enum TriggerTarget
    {
        Player,
        Enemy
    }

    public MeshRenderer meshRenderer;

    [Header("트리거에 들어올 대상")]
    public TriggerTarget triggerTarget = TriggerTarget.Player;

    [Header("트리거에 들어올 때 실행할 이벤트들")]
    public List<EventData> enterEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    [Header("반복 가능한지 체크")]
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
        if ((triggerTarget == TriggerTarget.Player && other.CompareTag("Player")) ||
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
