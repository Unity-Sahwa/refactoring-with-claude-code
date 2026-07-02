using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
public class KillTrigger : MonoBehaviour
{
    public List<Enemy> enemies = new List<Enemy>();
    public List<EventData> killEvents;
    public List<float> delayTimes;
    public bool isLoop = false;

    private bool hasTriggered = false;
    private void Start()
    {
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
                    delayTimes.Add(0f); 
                }
                for (var i = 0; i < killEvents.Count; i++)
                {
                    if (killEvents[i] != null)
                    {
                        StartCoroutine(ExecuteEventWithDelay(killEvents[i], delayTimes[i]));
                    }
                }
                hasTriggered = true; 
            }
        }        
    }

    private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventData.Execute();
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
}