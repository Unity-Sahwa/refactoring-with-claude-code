using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class Shake : IUIEffectLogic
    {
        public Type ConfigType => typeof(UIShakeConfig);

        // 로직 인스턴스 하나를 여러 UI가 같이 쓰기 때문에 대상별로 기억해야 함.
        private readonly Dictionary<GameObject, (RectTransform rectTransform, Vector2 origin)> _origins = new();

        public void Begin(GameObject target, UIEffectConfig config)
        {
            var rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            _origins[target] = (rectTransform, rectTransform.anchoredPosition);
        }

        public void Tick(GameObject target, UIEffectConfig config, float progress)
        {
            if (!_origins.TryGetValue(target, out (RectTransform rectTransform, Vector2 origin) saved)) return;

            var cfg = (UIShakeConfig)config;
            float t = progress * cfg.Duration * cfg.Frequency; // 흔들림 지도 위를 걸어간 거리

            // PerlinNoise에 의해 랜덤처럼 보이면서 부드럽게 이어지도록 함. x,y의 값은 (-1~1) * amplitude.
            float x = (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f * cfg.Amplitude;
            float y = (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f * cfg.Amplitude;
            saved.rectTransform.anchoredPosition = saved.origin + new Vector2(x, y);
        }

        public void End(GameObject target, UIEffectConfig config)
        {
            if (_origins.TryGetValue(target, out (RectTransform rectTransform, Vector2 origin) saved))
            {
                saved.rectTransform.anchoredPosition = saved.origin;
            }
            _origins.Remove(target);
        }
    }

    // 알파를 만지는 효과들의 공통부. 대상별 CanvasGroup을 기억해두고 시작/끝 알파만 자식이 정함.
    public abstract class FadeBase : IUIEffectLogic
    {
        public abstract Type ConfigType { get; }
        protected abstract float StartAlpha(UIEffectConfig config);
        protected abstract float EndAlpha(UIEffectConfig config);
        protected abstract float AlphaAt(UIEffectConfig config, float elapsed);

        private readonly Dictionary<GameObject, CanvasGroup> _groups = new();

        public void Begin(GameObject target, UIEffectConfig config)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null) return;

            canvasGroup.alpha = StartAlpha(config);
            _groups[target] = canvasGroup;
        }

        public void Tick(GameObject target, UIEffectConfig config, float progress)
        {
            if (!_groups.TryGetValue(target, out CanvasGroup canvasGroup)) return;

            canvasGroup.alpha = AlphaAt(config, progress * config.Duration);
        }

        public void End(GameObject target, UIEffectConfig config)
        {
            if (_groups.TryGetValue(target, out CanvasGroup canvasGroup))
            {
                canvasGroup.alpha = EndAlpha(config);
            }
            _groups.Remove(target);
        }
    }

    // 서서히 떴다가 서서히 사라짐
    public class FadeInOut : FadeBase
    {
        public override Type ConfigType => typeof(UIFadeInOutConfig);

        protected override float StartAlpha(UIEffectConfig config) => 0f;
        protected override float EndAlpha(UIEffectConfig config) => ((UIFadeInOutConfig)config).EndAlpha;

        protected override float AlphaAt(UIEffectConfig config, float elapsed)
        {
            var cfg = (UIFadeInOutConfig)config;
            float remaining = cfg.Duration - elapsed;

            if (elapsed < cfg.FadeInTime)
            {
                return elapsed / cfg.FadeInTime; // 서서히 뜸
            }
            if (remaining < cfg.FadeOutTime)
            {
                return remaining / cfg.FadeOutTime; // 서서히 사라짐
            }
            return 1f; // 가운데 구간은 그대로 보임
        }
    }

    public class FadeIn : FadeBase
    {
        public override Type ConfigType => typeof(UIFadeInConfig);

        protected override float StartAlpha(UIEffectConfig config) => 0f;
        protected override float EndAlpha(UIEffectConfig config) => 1f;
        protected override float AlphaAt(UIEffectConfig config, float elapsed) => elapsed / config.Duration;
    }

    public class FadeOut : FadeBase
    {
        public override Type ConfigType => typeof(UIFadeOutConfig);

        protected override float StartAlpha(UIEffectConfig config) => 1f;
        protected override float EndAlpha(UIEffectConfig config) => 0f;
        protected override float AlphaAt(UIEffectConfig config, float elapsed) => 1f - elapsed / config.Duration;
    }
}
