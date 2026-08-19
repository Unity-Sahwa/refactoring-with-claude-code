using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 플레이어 사망(OnPlayerDied) 신호를 받아 EventData 목록을 각자 delay만큼 기다렸다 실행.
    // EnterTrigger와 동일한 리스트+딜레이 구조를 사망 신호용으로 재사용한 것.
    public class PlayerDeathTrigger : MonoBehaviour
    {
        [Inject(true)] private List<PlayerDamageReceiver> _receivers;

        public List<EventData> deathEvents;
        public List<float> delayTimes;

        private void OnEnable()
        {
            if (_receivers == null) return;

            foreach (var receiver in _receivers)
            {
                receiver.OnPlayerDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (_receivers == null) return;

            foreach (var receiver in _receivers)
            {
                receiver.OnPlayerDied -= HandleDied;
            }
        }

        private void HandleDied()
        {
            while (delayTimes.Count < deathEvents.Count)
            {
                delayTimes.Add(0f);
            }

            for (int i = 0; i < deathEvents.Count; i++)
            {
                if (deathEvents[i] != null)
                {
                    StartCoroutine(ExecuteEventWithDelay(deathEvents[i], delayTimes[i]));
                }
            }
        }

        private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
        {
            yield return new WaitForSeconds(delay);
            eventData.Execute();
        }
    }
}
