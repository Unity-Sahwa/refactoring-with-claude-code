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

        [Preserve, Inject] private HitChannel _hitChannel;

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

            // 연속 피격이면 이전 점멸을 끊고 다시 시작한다.
            if (_running.TryGetValue(target, out Coroutine running))
            {
                StopCoroutine(running);
            }

            _running[target] = StartCoroutine(CoFlash(target, renderers));
        }

        private IEnumerator CoFlash(GameObject target, Renderer[] renderers)
        {
            // 렌더러마다 원래색이 다를 수 있어 각자 기억
            Color[] original = new Color[renderers.Length];
            Color[] originalEmission = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                original[i] = HasColor(renderers[i]) ? renderers[i].sharedMaterial.GetColor(BaseColorId) : Color.white;
                originalEmission[i] = HasEmission(renderers[i]) ? renderers[i].sharedMaterial.GetColor(EmissionColorId) : Color.black;
            }

            float half = _duration / (_blinkCount * 2);
            for (int i = 0; i < _blinkCount; i++)
            {
                Paint(renderers, _flashColor, null, null);
                yield return new WaitForSeconds(half);

                Paint(renderers, default, original, originalEmission);
                yield return new WaitForSeconds(half);
            }

            // 마지막에 한 번 더 덮어 원래색을 확실히 되돌린다.
            Paint(renderers, default, original, originalEmission);
            _running.Remove(target);
        }

        // original이 있으면 렌더러별 그 색으로, 없으면 flat 색으로 덮는다(기존 MPB 값 보존).
        private void Paint(Renderer[] renderers, Color flat, Color[] original, Color[] originalEmission)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                bool hasColor = HasColor(renderers[i]);
                bool hasEmission = HasEmission(renderers[i]);
                if (!hasColor && !hasEmission)
                {
                    continue;
                }

                renderers[i].GetPropertyBlock(_mpb);
                if (hasColor)
                {
                    _mpb.SetColor(BaseColorId, original != null ? original[i] : flat);
                }

                // ponytail: _EMISSION 키워드가 꺼진 머티리얼은 MPB로 못 켜서 무시됨. 필요하면 머티리얼에서 Emission 체크.
                if (hasEmission)
                {
                    _mpb.SetColor(EmissionColorId, originalEmission != null ? originalEmission[i] : flat * _emissionIntensity);
                }

                renderers[i].SetPropertyBlock(_mpb);
            }
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
