using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 역할: 처형 상태의 진행률 타이밍 이벤트를 받아 액션을 취한다(스턴, 처형)
    [RequireComponent(typeof(FinishTargetScanner))]
    public class FinishExecutor : MonoBehaviour
    {
        [SerializeField] private float _stunTime = 7f;   // 스턴(모션 정지) 시간
        [SerializeField] private float _executeHeal = 10f; // 처형 성공 시 회복량

        [Preserve, Inject] private IPlayerStateEventSubscriber _playerStateEventSubscriber;
        [Preserve, Inject] private IFinishTargetProvider _finishTargetProvider;
        [Preserve, Inject(true)] private IHealthInfo _healthInfo;
        [Preserve, Inject(true)] private IHealthModifier _healthModifier;

        private IDisposable _finishEventDisposable;

        private void Awake()
        {
            // 처형은 정리가 필요 없는 순수 원샷이라 close 없이 등록한다.
            _finishEventDisposable = _playerStateEventSubscriber?.Register(StateEventCategory.Finish, HandleFinish);
        }

        private void OnDestroy()
        {
            _finishEventDisposable?.Dispose();
        }

        private void HandleFinish(IStartData data)
        {
            if (data is not FinishDataEntry entry) return;

            switch (entry.Action)
            {
                case FinishActionType.Stun:    Stun();    break;
                case FinishActionType.Execute: Execute(); break;
            }
        }

        // 현재 범위 내 스턴 대상 전부를 모션 정지.
        private void Stun()
        {
            var targets = _finishTargetProvider?.GatherStunTargets();
            if (targets == null) return;

            for (int i = 0; i < targets.Count; i++)
            {
                Enemy enemy = targets[i];
                if (enemy != null && !enemy.isDead) enemy.MotionStop(_stunTime);
            }
        }

        // 현재 범위 내 대상 전부를 처형.
        private void Execute()
        {
            var targets = _finishTargetProvider?.GatherExecuteTargets();
            if (targets == null) return;

            for (int i = 0; i < targets.Count; i++)
            {
                Enemy enemy = targets[i];
                if (enemy != null && !enemy.isDead)
                {
                    enemy.Execution();
                }
            }

            Heal(_executeHeal);
        }

        // 처형 성공 1회당 소량 회복.
        private void Heal(float amount)
        {
            if (_healthInfo == null || _healthModifier == null || amount <= 0f) return;
            _healthModifier.SetCurrent(_healthInfo.Current + amount);
        }
    }
}
