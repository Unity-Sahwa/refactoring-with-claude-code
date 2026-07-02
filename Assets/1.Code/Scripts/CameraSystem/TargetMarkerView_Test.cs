using UnityEngine;
using UnityEngine.UI;

namespace Refactoring
{
    // D: 락온/탐지 대상 위에 마커(Image)를 띄운다. B·C를 "보고" 표시할 대상을 정해 화면 좌표로 옮긴다.
    // 부착 위치: 마커 Image가 붙은 오브젝트(자기 Image를 GetComponent로 쓴다).
    public class TargetMarkerView_Test : MonoBehaviour
    {
        // ponytail: 색 상수(원본 CameraData 값). 추후 SO로 이주.
        private static readonly Color DetectedColor = new Color(1f, 0f, 0f, 0.2f); // 탐지(약하게)
        private static readonly Color LockOnColor = new Color(1f, 0f, 0f, 1f);     // 락온(진하게)

        [Inject] private ITargetDetector _detector;
        [Inject] private ILockOnState _lockOn;
        [Inject] private ILockOnTarget _lockOnTarget;

        private Image _marker;
        private Collider _shown;          // 지금 마커가 따라가는 대상
        private Transform _markerPoint;   // 그 대상의 "MarkerPoint" 자식(없으면 대상 자신)

        private void Awake()
        {
            _marker = GetComponent<Image>();
        }

        private void Update()
        {
            Collider target = PickTarget(out bool isLockOn);

            if (target == null)
            {
                if (_marker.enabled) _marker.enabled = false;
                _shown = null;
                _markerPoint = null;
                return;
            }

            // 대상이 바뀔 때만 MarkerPoint를 다시 찾는다(매 프레임 Find 방지).
            if (target != _shown)
            {
                _shown = target;
                Transform point = target.transform.Find("MarkerPoint");
                _markerPoint = point != null ? point : target.transform;
            }

            _marker.enabled = true;
            _marker.color = isLockOn ? LockOnColor : DetectedColor;

            if (Camera.main != null)
            {
                _marker.rectTransform.position = Camera.main.WorldToScreenPoint(_markerPoint.position);
            }
        }

        // 락온 대상이 있으면 그게 우선, 없으면 탐지 대상.
        private Collider PickTarget(out bool isLockOn)
        {
            if (_lockOn != null && _lockOn.IsLockOn && _lockOnTarget?.LockedTarget != null)
            {
                isLockOn = true;
                return _lockOnTarget.LockedTarget;
            }
            isLockOn = false;
            return _detector?.CurrentTarget;
        }
    }
}
