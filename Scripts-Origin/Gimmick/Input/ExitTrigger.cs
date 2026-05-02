using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    public enum TriggerTarget
    {
        Player,
        Enemy
    }
    public MeshRenderer meshRenderer;

    [Header("트리거에서 나갈 대상")]
    public TriggerTarget triggerTarget = TriggerTarget.Player;

    [Header("트리거에서 나갈 때 실행할 이벤트들")]
    public List<EventData> exitEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    [Header("반복 가능한지 체크")]
    public bool isLoop = false;



    private bool hasTriggered = false;

    private void Start()
    {
        meshRenderer.enabled = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (hasTriggered && !isLoop)
        {
            return;
        }

        if ((triggerTarget == TriggerTarget.Player && other.CompareTag("Player")) ||
            (triggerTarget == TriggerTarget.Enemy && other.CompareTag("Enemy")))
        {
            while (delayTimes.Count < exitEvents.Count)
            {
                delayTimes.Add(0f);
            }
            for (int i = 0; i < exitEvents.Count; i++)
            {
                if (exitEvents[i] != null)
                {
                    StartCoroutine(ExecuteEventWithDelay(exitEvents[i], delayTimes[i]));
                }
            }
        }
        hasTriggered = true;
    }
    private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventData.Execute();
    }
}
