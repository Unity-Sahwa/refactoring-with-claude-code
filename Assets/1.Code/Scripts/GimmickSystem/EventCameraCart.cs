using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 스플라인 위에 배치된 CinemachineSplineCart를 트리거 시점에 움직여 인디케이터(Trail)를 이동시킨다.
    // 카트에는 TrailRenderer를 붙여두면 된다. 평소엔 이 컴포넌트가 꺼져 있어야 한다.
    public class EventCameraCart : EventData
    {
        [SerializeField] private CinemachineSplineCart _cart;

        private void Awake()
        {   
            _cart.gameObject.SetActive(false);
        }

        public override void Execute()
        {
            if (_cart == null)
            {
                return;
            }
            _cart.gameObject.SetActive(false);
            _cart.SplinePosition = 0;
            _cart.gameObject.SetActive(true);
        }
    }
}
