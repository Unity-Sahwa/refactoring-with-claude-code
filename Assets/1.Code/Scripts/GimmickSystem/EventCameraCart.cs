using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 스플라인 위에 배치된 CinemachineSplineCart를 트리거 시점에 움직여 인디케이터(Trail)를 이동시킨다.
    // 카트에는 TrailRenderer를 붙여두면 된다. 평소엔 이 컴포넌트가 꺼져 있어야 한다.
    public class EventCameraCart : EventData
    {
        [SerializeField] private CinemachineSplineCart _cart;
        [SerializeField] private TrailRenderer trail;

        private void Awake()
        {
            _cart.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _cart.SplinePosition = 0;
        }

        public override void Execute()
        {
            if (_cart == null)
            {
                return;
            }

            // 순간이동 궤적이 트레일에 찍히지 않도록 기록 자체를 막아둔다.
            if (trail != null)
            {
                trail.emitting = false;
            }

            _cart.gameObject.SetActive(false);
            _cart.SplinePosition = 0;
            _cart.gameObject.SetActive(true);

            if (trail != null)
            {
                StartCoroutine(ResumeTrailNextFrame());
            }
        }

        private IEnumerator ResumeTrailNextFrame()
        {
            // 카트가 새 위치로 실제 스냅될 때까지(다음 프레임) 대기 후 재개한다.
            yield return null;
            trail.Clear();
            trail.emitting = true;
        }
    }
}
