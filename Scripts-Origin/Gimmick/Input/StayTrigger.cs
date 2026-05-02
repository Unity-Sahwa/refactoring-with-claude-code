using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayTrigger : MonoBehaviour
{
    public enum TriggerTarget
    {
        Player,
        Enemy
    }
    public MeshRenderer meshRenderer;

    [Header("트리거에 들어와 있을 대상")]
    public TriggerTarget triggerTarget = TriggerTarget.Player;

    [Header("트리거에 들어와 있을 때 실행할 이벤트들")]
    public List<EventData> stayEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    [Header("반복 가능한지 체크")]
    public bool isLoop = false; 

    private bool hasTriggered = false;

    private void Start()
    {
        meshRenderer.enabled = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasTriggered && !isLoop)
        {
            return;
        }

        if ((triggerTarget == TriggerTarget.Player && other.CompareTag("Player")) ||
            (triggerTarget == TriggerTarget.Enemy && other.CompareTag("Enemy")))
        {
            while (delayTimes.Count < stayEvents.Count)
            {
                delayTimes.Add(0f);
            }
            for (int i = 0; i < stayEvents.Count; i++)
            {
                if (stayEvents[i] != null)
                {
                    StartCoroutine(ExecuteEventWithDelay(stayEvents[i], delayTimes[i]));
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
