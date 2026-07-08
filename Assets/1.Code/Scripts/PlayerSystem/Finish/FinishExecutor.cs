using UnityEngine;

namespace Refactoring
{
    // 역할: 처형 상태의 진행률 타이밍 이벤트를 받아 액션을 취한다(스턴, 처형)
    [RequireComponent(typeof(FinishTargetScanner))]
    public class FinishExecutor : MonoBehaviour
    {
        [SerializeField] private float _stunTime = 7f;   // 스턴(모션 정지) 시간

        [Inject] private IPlayerStateEventSubscriber _playerStateEventSubscriber;
        [Inject] private IFinishTargetProvider _finishTargetProvider;

        private void Awake()
        {
            _playerStateEventSubscriber?.Subscribe(StateEventCategory.Finish, HandleFinish);
        }

        private void OnDestroy()
        {
            if (_playerStateEventSubscriber != null)
            {
                _playerStateEventSubscriber.Unsubscribe(StateEventCategory.Finish, HandleFinish);
            }
        }

        private void HandleFinish(PlayerCharacter source, IStartData data)
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
                if (enemy != null && !enemy.isDead) enemy.Execution();
            }
        }
    }
}
