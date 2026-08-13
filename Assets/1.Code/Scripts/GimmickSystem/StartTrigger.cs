using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 씬이 시작할 때 실행할 이벤트를 모아 두는 곳. 플레이어를 기다리지 않는다는 점만 EnterTrigger와 다르다.
    // 씬을 어떻게 들어왔는지에 따라 실행 여부가 갈린다. 불러오기로 들어온 씬에서 시작 연출이 자리를 덮는 사고를 막는다.
    public class StartTrigger : MonoBehaviour
    {
        public enum SceneStartMode
        {
            // 어떻게 들어왔든 실행한다.
            Always,

            // 불러오기가 아닌 모든 진입. 앞 지역에서 넘어온 경우와 새 게임으로 처음 들어온 경우가 여기 해당한다.
            OnAreaEnter,

            // 세이브 슬롯을 불러와 들어온 경우. 낙사 복귀로 씬을 다시 띄운 것도 여기 해당한다.
            OnSlotLoad,
        }

        [SerializeField] private SceneStartMode _runOn = SceneStartMode.OnAreaEnter;
        [SerializeField] private List<EventData> _startEvents;
        [SerializeField] private List<float> _delayTimes;

        private void Start()
        {
            if (!ShouldRun())
            {
                return;
            }

            for (int i = 0; i < _startEvents.Count; i++)
            {
                if (_startEvents[i] == null)
                {
                    continue;
                }

                // 지연을 안 적은 칸은 0으로 본다. EnterTrigger처럼 목록 길이를 맞추지 않고 그때그때 읽는다.
                float delay = i < _delayTimes.Count ? _delayTimes[i] : 0f;
                StartCoroutine(ExecuteEventWithDelay(_startEvents[i], delay));
            }
        }

        private bool ShouldRun()
        {
            // sceneLoaded가 Start보다 먼저라, 이 값은 여기서 읽는 시점에 이미 정해져 있다.
            bool fromSlot = SlotLoadRunner.LoadedFromSlot;

            return _runOn switch
            {
                SceneStartMode.OnAreaEnter => !fromSlot,
                SceneStartMode.OnSlotLoad => fromSlot,
                _ => true,
            };
        }

        private IEnumerator ExecuteEventWithDelay(EventData eventData, float delay)
        {
            yield return new WaitForSeconds(delay);
            eventData.Execute();
        }
    }
}
