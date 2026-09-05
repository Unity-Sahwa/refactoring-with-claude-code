using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 플레이어 사망 신호를 받아 EventData 목록을 각자 지연시간만큼 기다렸다 실행한다. (EnterTrigger와 같은 구조)
    public class PlayerDeathTrigger : MonoBehaviour
    {
        [Preserve, Inject(true)] private List<PlayerDamageReceiver> _receivers;

        [SerializeField] private List<EventData> _deathEvents;
        [Tooltip("각 이벤트를 실행하기 전 기다릴 시간(초). 모자라면 0으로 채운다")]
        [SerializeField] private List<float> _delayTimes;

        private void OnEnable()
        {
            if (_receivers == null)
            {
                return;
            }

            foreach (var receiver in _receivers)
            {
                receiver.OnPlayerDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (_receivers == null)
            {
                return;
            }

            foreach (var receiver in _receivers)
            {
                receiver.OnPlayerDied -= HandleDied;
            }
        }

        private void HandleDied()
        {
            while (_delayTimes.Count < _deathEvents.Count)
            {
                _delayTimes.Add(0f);
            }

            for (int i = 0; i < _deathEvents.Count; i++)
            {
                if (_deathEvents[i] != null)
                {
                    StartCoroutine(CoExecuteEventWithDelay(_deathEvents[i], _delayTimes[i]));
                }
            }
        }

        private IEnumerator CoExecuteEventWithDelay(EventData eventData, float delay)
        {
            yield return new WaitForSeconds(delay);
            eventData.Execute();
        }
    }
}
