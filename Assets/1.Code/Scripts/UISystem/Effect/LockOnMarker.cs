using UnityEngine;

namespace Refactoring
{
    // 역할: 지금 조준 중인 적 머리 위에 마커를 띄운다. 락온 전이면 반투명, 락온되면 선명하게.
    [RequireComponent(typeof(CanvasGroup))]
    public class LockOnMarker : MonoBehaviour
    {
        [Tooltip("적 머리끝에서 얼마나 더 위에 띄울지(월드 단위)")]
        [SerializeField]
        private float _heightOffset = 0.3f;

        [Tooltip("락온 전(후보일 때) 투명도")]
        [SerializeField, Range(0f, 1f)]
        private float _aimAlpha = 0.4f;

        [Inject]
        private LockOnController _lockOn;

        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private Camera _mainCamera;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void LateUpdate()
        {
            Collider target = _lockOn?.AimTarget;
            if (target == null)
            {
                _canvasGroup.alpha = 0f;
                return;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return;
                }
            }

            // bounds는 콜라이더를 감싸는 상자. max.y가 머리끝이다.
            Vector3 headPoint = target.bounds.center;
            headPoint.y = target.bounds.max.y + _heightOffset;

            Vector3 screenPoint = _mainCamera.WorldToScreenPoint(headPoint);

            // z가 음수면 카메라 뒤쪽이라 좌표가 뒤집힌 채로 화면에 찍힌다. 그냥 숨긴다.
            if (screenPoint.z <= 0f)
            {
                _canvasGroup.alpha = 0f;
                return;
            }

            _rect.position = screenPoint;
            _canvasGroup.alpha = _lockOn.IsLockOn ? 1f : _aimAlpha;
        }
    }
}
