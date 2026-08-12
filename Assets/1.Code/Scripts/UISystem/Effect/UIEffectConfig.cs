using System;
using UnityEngine;

namespace Refactoring
{
    // 역할: 각 UI 이펙트마다의 수치 묶음.
    
    [Serializable]
    public abstract class UIEffectConfig
    {
        [SerializeField] protected float duration = 0.5f;
        public float Duration => duration;
        
        [SerializeField] protected bool deactivateOnStop = true; // UI 이펙트 끝날 때 대상을 비활성화(디폴트). 체력 HUD처럼 끝나도 계속 보여야 하는 건 false
        public bool DeactivateOnStop => deactivateOnStop;
    }

    [Serializable]
    public class UIShakeConfig : UIEffectConfig
    {
        [SerializeField] private float amplitude = 10f; // 흔들리는 크기(픽셀)
        [SerializeField] private float frequency = 25f; // 초당 흔들리는 빠르기

        public float Amplitude => amplitude;
        public float Frequency => frequency;
    }

    // 서서히 떴다 서서히 사라지는 수치
    [Serializable]
    public class UIFadeInOutConfig : UIEffectConfig
    {
        [SerializeField] private float fadeInTime = 0.3f;  // 서서히 뜨는 시간
        [SerializeField] private float fadeOutTime = 0.3f; // 서서히 사라지는 시간
        [SerializeField] private float endAlpha = 0f;      // 끝났을 때 남길 투명도

        public float FadeInTime => fadeInTime;
        public float FadeOutTime => fadeOutTime;
        public float EndAlpha => endAlpha;
    }

    // Duration 동안 alpha 0 -> 1
    [Serializable]
    public class UIFadeInConfig : UIEffectConfig { }

    // Duration 동안 alpha 1 -> 0
    [Serializable]
    public class UIFadeOutConfig : UIEffectConfig { }
}
