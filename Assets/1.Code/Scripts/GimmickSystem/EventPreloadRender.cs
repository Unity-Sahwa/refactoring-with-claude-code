using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 역할: 맵 전체를 비추는 카메라로 오브젝트/몬스터/이펙트를 미리 한 번 렌더해 첫 등장 스터터를 없앤다.
    // 불이 꺼지는 씬은 셰이더 keyword가 갈리므로 불 켠 상태와 끈 상태를 각각 렌더한다.
    public class EventPreloadRender : EventData
    {
        [SerializeField] private Camera _previewCamera;
        [SerializeField] private Light _light;

        // 불 켠 상태와 끈 상태에 각각 적용되는 대기 시간. 파티클이 실제로 뿜어져 나올 시간을 준다.
        [SerializeField] private float _holdSeconds = 0.2f;

        [Preserve, Inject] private List<IPreloadTargetProvider> _targetProviders;

        public override void Execute()
        {
            StartCoroutine(PreloadRoutine());
        }

        private IEnumerator PreloadRoutine()
        {
            // 풀 인스턴스는 Awake에서 만들어지므로 Execute 시점(Start)에 모아도 늦지 않다.
            var targets = new List<GameObject>();
            if (_targetProviders != null)
            {
                foreach (var provider in _targetProviders)
                {
                    targets.AddRange(provider.PreloadTargets);
                }
            }

            bool[] wasActive = new bool[targets.Count];
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null) continue;

                wasActive[i] = targets[i].activeSelf;
                targets[i].SetActive(true);
            }

            if (_previewCamera != null)
            {
                _previewCamera.gameObject.SetActive(true);
            }

            // 켜진 프레임이 실제로 그려질 때까지 기다린다.
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(_holdSeconds);

            // Light를 안 꽂았으면 불이 안 꺼지는 씬으로 보고 위의 렌더 한 번으로 끝낸다.
            // 꽂혀 있어도 이미 intensity가 0이면 추가로 렌더할 상태가 없다.
            if (_light != null && _light.intensity > 0f)
            {
                float intensity = _light.intensity;
                _light.intensity = 0f;

                yield return new WaitForEndOfFrame();
                yield return null;

                _light.intensity = intensity;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null) continue;

                targets[i].SetActive(wasActive[i]);
            }

            if (_previewCamera != null)
            {
                _previewCamera.gameObject.SetActive(false);
            }
        }
    }
}
