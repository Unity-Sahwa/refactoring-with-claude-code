using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTrigger : MonoBehaviour
{
    [Header("목표 위치")]
    public Vector3 targetPosition;

    [Header("허용 오차 거리")]
    public float tolerance = 0.1f;

    [Header("목표 위치에 도달했을 때 실행할 이벤트들")]
    public List<EventData> positionReachedEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    private void Update()
    {
        if (Vector3.Distance(this.transform.position, targetPosition) <= tolerance)
        {
            StartCoroutine(ExecuteEventsWithDelay());
            enabled = false;
        }
    }

    private IEnumerator ExecuteEventsWithDelay()
    {
        while (delayTimes.Count < positionReachedEvents.Count)
        {
            delayTimes.Add(0f);
        }
        for (int i = 0; i < positionReachedEvents.Count; i++)
        {
            if (positionReachedEvents[i] != null)
            {
                yield return new WaitForSeconds(delayTimes[i]);
                positionReachedEvents[i].Execute();
            }
        }
    }
}
