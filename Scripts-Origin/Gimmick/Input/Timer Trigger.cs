using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerTrigger : EventData
{
    [Header("타이머 UI 텍스트")]
    public Text timerText;

    [Header("타이머 지속 시간")]
    [Range(1, 999)] public int duration = 10;

    [Header("타이머 일시정지 키")]
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    [Header("타이머 종료 시 실행할 이벤트")]
    public List<EventData> TimeUpEvents;

    private Coroutine timerCoroutine;
    private bool isPaused = false;
    private float remainingTime;

    public override void Execute()
    {
        if (timerText != null && timerCoroutine == null)
        {
            remainingTime = duration;
            timerCoroutine = StartCoroutine(TimerCoroutine());
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (remainingTime > 0)
        {
            if (!isPaused)
            {
                remainingTime -= Time.deltaTime;
                // 남은 시간을 UI에 표시 (소수점 2자리)
                timerText.text = remainingTime.ToString("F2");
            }

            if (Input.GetKeyDown(pauseKey))
            {
                isPaused = !isPaused;
            }

            yield return null;
        }

        timerText.text = "0.00";
        StartCoroutine(ExecuteEventsWithDelay());
        timerCoroutine = null;
    }

    private IEnumerator ExecuteEventsWithDelay()
    {
        while (delayTimes.Count < TimeUpEvents.Count)
        {
            delayTimes.Add(0f);
        }
        for (int i = 0; i < TimeUpEvents.Count; i++)
        {
            if (TimeUpEvents[i] != null)
            {
                yield return new WaitForSeconds(delayTimes[i]);
                TimeUpEvents[i].Execute();
            }
        }
    }
}
