using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour, IDamageable
{
    [Header("공격 당하는 횟수")]
    public float objectHP = 1;

    [Header("피격 쿨타임")]
    public float MIN_TIME_BET_DAMAGE = 1f;

    [Header("부서질 때 실행할 이벤트들")]
    public List<EventData> brokenEvents;

    [Header("각 이벤트의 지연 시간 (초)")]
    public List<float> delayTimes;

    private float lastDamagedTime = 0f;

    private bool isDead = false;

    private EventData eventData;

    public bool ApplyDamage(DamageMessage damageMessage)
    {
        if (Time.time < lastDamagedTime + MIN_TIME_BET_DAMAGE || isDead)
        {
            return false;
        }

        lastDamagedTime = Time.time;
        objectHP--;


        if (objectHP <= 0)
        {
            for (int i = 0; i < brokenEvents.Count; i++)
            {
                if (brokenEvents[i] != null)
                {
                    StartCoroutine(ExecuteEventWithBroken(brokenEvents[i], delayTimes[i]));
                }
            }
        }
        return true;
    }

    private IEnumerator ExecuteEventWithBroken(EventData eventData, float delay)
    {
        yield return new WaitForSeconds(delay);
        eventData.Execute();
    }

    public void DieAction()
    {

    }
}
