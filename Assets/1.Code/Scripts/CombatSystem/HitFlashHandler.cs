using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: HitChannel을 구독해, 맞은 몬스터를 잠깐 빨강으로 점멸시킨다.
    public class HitFlashHandler : MonoBehaviour
    {
        [Inject] private HitChannel _hitChannel;
        [SerializeField] private Color _flashColor = Color.red;
        [SerializeField] private float _duration = 0.3f; // 총 점멸 시간
        [SerializeField] private int _blinkCount = 1;    // 빨강↔원래색 왕복 횟수
        [SerializeField] private float _emissionIntensity = 1f; // Emission 발광 세기

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private readonly Dictionary<GameObject, Coroutine> _running = new();
        private MaterialPropertyBlock _mpb; // Awake에서 생성(생성자에서 new 금지)
        private IDisposable _hitDisposable;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            _hitDisposable = _hitChannel.Register(HandleHit);
        }
        private void OnDestroy() => _hitDisposable?.Dispose();

        private void HandleHit(HitReport r)
        {
            GameObject target = r.Target;
            if (target == null) return;

            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            if (_running.TryGetValue(target, out var c)) StopCoroutine(c); // 연속 피격이면 재시작
            _running[target] = StartCoroutine(FlashRoutine(target, renderers));
        }

        private IEnumerator FlashRoutine(GameObject target, Renderer[] renderers)
        {
            // 렌더러마다 원래색이 다를 수 있어 각자 기억
            var original = new Color[renderers.Length];
            var originalEmission = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                original[i] = HasColor(renderers[i]) ? renderers[i].sharedMaterial.GetColor(BaseColorId) : Color.white;
                originalEmission[i] = HasEmission(renderers[i]) ? renderers[i].sharedMaterial.GetColor(EmissionColorId) : Color.black;
            }

            float half = _duration / (_blinkCount * 2);
            for (int b = 0; b < _blinkCount; b++)
            {
                Paint(renderers, _flashColor, null, null);   // 빨강
                yield return new WaitForSeconds(half);
                Paint(renderers, default, original, originalEmission);    // 원래색
                yield return new WaitForSeconds(half);
            }

            Paint(renderers, default, original, originalEmission); // 끝에 확실히 원복
            _running.Remove(target);
        }

        // original이 있으면 렌더러별 그 색으로, 없으면 flat 색으로 덮는다(기존 MPB 값 보존).
        private void Paint(Renderer[] renderers, Color flat, Color[] original, Color[] originalEmission)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                bool hasColor = HasColor(renderers[i]);
                bool hasEmission = HasEmission(renderers[i]);
                if (!hasColor && !hasEmission) continue;

                renderers[i].GetPropertyBlock(_mpb);
                if (hasColor) _mpb.SetColor(BaseColorId, original != null ? original[i] : flat);
                // ponytail: _EMISSION 키워드가 꺼진 머티리얼은 MPB로 못 켜서 무시됨. 필요하면 머티리얼에서 Emission 체크.
                if (hasEmission) _mpb.SetColor(EmissionColorId, originalEmission != null ? originalEmission[i] : flat * _emissionIntensity);
                renderers[i].SetPropertyBlock(_mpb);
            }
        }

        private static bool HasColor(Renderer r) =>
            r != null && r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorId);

        private static bool HasEmission(Renderer r) =>
            r != null && r.sharedMaterial != null && r.sharedMaterial.HasProperty(EmissionColorId);
    }
}
