using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [Header("적 몬스터를 집어넣는 곳")]
    public List<Enemy> enemies = new List<Enemy>();

    [Header("실행할 이벤트들")]
    public List<EventData> killEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    [Header("반복 가능한지 체크")]
    public bool isLoop = false;

    private bool hasTriggered = false;
    private void Start()
    {
        // 적 리스트가 비어있는지 확인
        if (enemies.Count == 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    public void OnEnemyKilled(Enemy enemy)
    {
        if (hasTriggered && !isLoop)
        {
            return; 
        }

        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);

            if (enemies.Count == 0)
            {
                while (delayTimes.Count < killEvents.Count)
                {
                    delayTimes.Add(0f); // 이벤트 수와 지연 시간 수를 맞추기 위한 처리
                }
                for (var i = 0; i < killEvents.Count; i++)
                {
                    if (killEvents[i] != null)
                    {
                        StartCoroutine(ExecuteEventWithDelay(killEvents[i], delayTimes[i]));
                    }
                }
                hasTriggered = true; // 트리거 발동 표시
            }
        }        
    }

    private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventData.Execute();
    }

    // 반복 트리거용 리셋 함수
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
