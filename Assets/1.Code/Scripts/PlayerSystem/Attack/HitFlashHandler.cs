using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: HitChannel을 구독해, 맞은 대상을 잠깐 점멸시킨다.
    // 흐름: 타격 신호 수신 → 대상 렌더러 원래색 기억 → 점멸색↔원래색 반복 → 원래색 복구
    public class HitFlashHandler : MonoBehaviour
    {
        // 셰이더 프로퍼티 ID는 프로퍼티 이름당 하나뿐인 전역 값이라 타입에 속한다.
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Preserve, Inject] private IHitEventSubscriber _hitChannel;

        [Tooltip("점멸할 때 덮어씌우는 색")]
        [SerializeField] private Color _flashColor = Color.red;

        [Tooltip("총 점멸 시간 (초)")]
        [SerializeField] private float _duration = 0.3f;

        [Tooltip("점멸색↔원래색 왕복 횟수")]
        [SerializeField] private int _blinkCount = 1;

        [Tooltip("Emission 발광 세기")]
        [SerializeField] private float _emissionIntensity = 1f;

        private readonly Dictionary<GameObject, Coroutine> _running = new();

        // MaterialPropertyBlock은 유니티 객체라 필드 초기화가 아니라 Awake에서 생성한다.
        private MaterialPropertyBlock _mpb;
        private IDisposable _hitDisposable;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _hitDisposable = _hitChannel.Register(HandleHit);
        }

        private void OnDestroy()
        {
            _hitDisposable?.Dispose();
        }

        private void HandleHit(HitReport report)
        {
            GameObject target = report.Target;
            if (target == null)
            {
                return;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            StopExistingFlash(target);
            _running[target] = StartCoroutine(CoFlash(target, renderers));
        }

        // 연속 피격이면 이전 점멸을 끊고 다시 시작한다.
        private void StopExistingFlash(GameObject target)
        {
            if (_running.TryGetValue(target, out Coroutine running))
            {
                StopCoroutine(running);
            }
        }

        private IEnumerator CoFlash(GameObject target, Renderer[] renderers)
        {
            CaptureOriginalColors(renderers, out Color[] original, out Color[] originalEmission);
            PaintColors flashColors = new PaintColors(_flashColor, null, null);
            PaintColors originalColors = new PaintColors(default, original, originalEmission);

            float half = _duration / (_blinkCount * 2);
            for (int i = 0; i < _blinkCount; i++)
            {
                Paint(renderers, flashColors);
                yield return new WaitForSeconds(half);

                Paint(renderers, originalColors);
                yield return new WaitForSeconds(half);
            }

            // 마지막에 한 번 더 덮어 원래색을 확실히 되돌린다.
            Paint(renderers, originalColors);
            _running.Remove(target);
        }

        // 렌더러마다 원래색이 다를 수 있어 각자 기억
        private void CaptureOriginalColors(Renderer[] renderers, out Color[] original, out Color[] originalEmission)
        {
            original = new Color[renderers.Length];
            originalEmission = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                original[i] = HasColor(renderers[i]) ? renderers[i].sharedMaterial.GetColor(BaseColorId) : Color.white;
                originalEmission[i] = HasEmission(renderers[i]) ? renderers[i].sharedMaterial.GetColor(EmissionColorId) : Color.black;
            }
        }

        // 페인트에 쓸 색 묶음. original이 있으면 렌더러별 그 색으로, 없으면 flat 색으로 덮는다.
        private readonly struct PaintColors
        {
            public readonly Color Flat;
            public readonly Color[] Original;
            public readonly Color[] OriginalEmission;

            public PaintColors(Color flat, Color[] original, Color[] originalEmission)
            {
                Flat = flat;
                Original = original;
                OriginalEmission = originalEmission;
            }
        }

        private void Paint(Renderer[] renderers, PaintColors colors)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                PaintRenderer(renderers[i], colors, i);
            }
        }

        private void PaintRenderer(Renderer renderer, PaintColors colors, int index)
        {
            if (renderer == null)
            {
                return;
            }

            bool hasColor = HasColor(renderer);
            bool hasEmission = HasEmission(renderer);
            if (!hasColor && !hasEmission)
            {
                return;
            }

            renderer.GetPropertyBlock(_mpb);
            ApplyColor(hasColor, colors, index);
            ApplyEmission(hasEmission, colors, index);
            renderer.SetPropertyBlock(_mpb);
        }

        private void ApplyColor(bool hasColor, PaintColors colors, int index)
        {
            if (!hasColor)
            {
                return;
            }

            _mpb.SetColor(BaseColorId, colors.Original != null ? colors.Original[index] : colors.Flat);
        }

        // ponytail: _EMISSION 키워드가 꺼진 머티리얼은 MPB로 못 켜서 무시됨. 필요하면 머티리얼에서 Emission 체크.
        private void ApplyEmission(bool hasEmission, PaintColors colors, int index)
        {
            if (!hasEmission)
            {
                return;
            }

            _mpb.SetColor(EmissionColorId, colors.OriginalEmission != null ? colors.OriginalEmission[index] : colors.Flat * _emissionIntensity);
        }

        private bool HasColor(Renderer renderer)
        {
            return renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty(BaseColorId);
        }

        private bool HasEmission(Renderer renderer)
        {
            return renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty(EmissionColorId);
        }
    }
}
