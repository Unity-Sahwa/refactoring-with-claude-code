using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventAnimationStart : EventData
{
    public enum AnimationMode
    {
        Play,
        CrossFade
    }

    [Header("애니메이션을 제어할 대상")]
    public Animator targetAnimator;

    [Header("애니메이션 상태 이름")]
    public string animationState;

    [Header("전환 방식")]
    public AnimationMode mode = AnimationMode.Play;

    [Header("CrossFade 전환 시간")]
    [Range(0.1f, 10f)] public float transitionDuration = 0.25f;

    public override void Execute()
    {
        if (targetAnimator != null && !string.IsNullOrEmpty(animationState))
        {
            switch (mode)
            {
                case AnimationMode.Play:
                    targetAnimator.Play(animationState);
                    break;
                case AnimationMode.CrossFade:
                    targetAnimator.CrossFade(animationState, transitionDuration);
                    break;
            }
        }
    }
}
