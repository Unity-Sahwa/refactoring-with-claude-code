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

    public class FadeInOut : IUIEffectLogic
    {
        public Type ConfigType => typeof(UIFadeConfig);

        private readonly Dictionary<GameObject, CanvasGroup> _groups = new();

        public void Begin(GameObject target, UIEffectConfig config)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null) return;

            canvasGroup.alpha = 0f;
            _groups[target] = canvasGroup;
        }

        public void Tick(GameObject target, UIEffectConfig config, float progress)
        {
            if (!_groups.TryGetValue(target, out CanvasGroup canvasGroup)) return;

            var cfg = (UIFadeConfig)config;
            float elapsed = progress * cfg.Duration;
            float remaining = cfg.Duration - elapsed;

            if (elapsed < cfg.FadeInTime)
            {
                canvasGroup.alpha = elapsed / cfg.FadeInTime; // 서서히 뜸
            }
            else if (remaining < cfg.FadeOutTime)
            {
                canvasGroup.alpha = remaining / cfg.FadeOutTime; // 서서히 사라짐
            }
            else
            {
                canvasGroup.alpha = 1f; // 가운데 구간은 그대로 보임
            }
        }

        public void End(GameObject target, UIEffectConfig config)
        {
            if (_groups.TryGetValue(target, out CanvasGroup canvasGroup))
            {
                canvasGroup.alpha = 0f;
            }
            _groups.Remove(target);
        }
    }
}
