using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventUnLock : EventData
{
    [Header("잠금 해제할 EventLock")]
    public EventLock eventLock; // EventLock에서 넘겨받을 객체들을 지정

    public override void Execute()
    {
        foreach (var target in eventLock.lockedTargets)
        {
            if (target != null)
            {
                // MonoBehaviour를 다시 활성화
                var components = target.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    component.enabled = true;
                }

                // PlayerController 다시 활성화
                var playerController = target.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.enabled = true;
                }

                // Enemy MotionStop을 풀기 위해 false로 설정
                var enemy = target.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.MotionStop(0f);
                }
            }
        }

        // 리스트 초기화 (다시 잠기는 것을 방지)
        eventLock.lockedTargets.Clear();
    }
}
