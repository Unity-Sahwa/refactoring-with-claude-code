using UnityEngine;

namespace Refactoring
{
    // 책임: "현재 애니가 어디까지 재생됐나"만 추적하여 상태/머신에 진행률과 종료 판정만 전달한다.
    //       (기존 CharacterBaseState.UpdateState에 섞여 있던 애니 분기 로직을 떼어냄)
    public class AnimationTracker
    {
        private Animator _animator;
        private int _animationHash;
        private bool _hasStarted;

        public float Progress { get; private set; }
        public bool IsFinished { get; private set; }

        public void Begin(Animator animator, int animationHash)
        {
            _animator = animator;
            _animationHash = animationHash;
            _hasStarted = false;
            IsFinished = false;
            Progress = 0f;
        }

        public void Update()
        {
            if (IsFinished)
            {
                return;
            }

            if (_animator.IsInTransition(0))
            {
                AnimatorStateInfo nextInfo = _animator.GetNextAnimatorStateInfo(0);

                // 현재 애니에서 다음 애니로 전환됨 → 종료
                if (nextInfo.shortNameHash != _animationHash)
                {
                    if (_hasStarted)
                    {
                        IsFinished = true;
                    }
                    return;
                }

                // 이전 애니에서 현재 애니로 전환되는 중
                Progress = Mathf.Clamp01(nextInfo.normalizedTime);
            }
            else
            {
                AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);

                // 이전 애니가 남아있거나 머신을 거치지 않고 다른 애니가 재생된 경우 → 종료
                if (info.shortNameHash != _animationHash)
                {
                    if (_hasStarted)
                    {
                        IsFinished = true;
                    }
                    return;
                }

                _hasStarted = true;
                Progress = Mathf.Clamp01(info.normalizedTime);
            }
        }
    }
}
