using UnityEngine;
using UnityEngine.EventSystems;

namespace Refactoring
{
    // 모바일 이동 조이스틱. 배경(자기 RectTransform) 안에서 손가락 위치를 방향값으로 바꿔 InputHub로 보냄.
    // 뗄 때는 0을 보내서 이동을 멈춤.
    public class MobileInputJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _handle;   // 눈에 보이는 손잡이. 없어도 동작함
        [SerializeField] private float _radius = 80f;     // 손잡이가 움직일 수 있는 반지름(픽셀)

        [Inject(true)] private IInputSender _inputSender;

        private RectTransform _area;

        private void Awake() => _area = (RectTransform)transform;

        public void OnPointerDown(PointerEventData eventData) => Move(eventData);
        public void OnDrag(PointerEventData eventData) => Move(eventData);

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_handle != null)
            {
                _handle.anchoredPosition = Vector2.zero;
            }
            _inputSender?.RouteMove(Vector2.zero);
        }

        private void Move(PointerEventData eventData)
        {
            // 손가락 화면 좌표를 조이스틱 배경 기준의 로컬 좌표로 바꿈
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _area, eventData.position, eventData.pressEventCamera, out Vector2 local);

            // 반지름을 넘으면 잘라내서 최대 1로 맞춤
            Vector2 value = Vector2.ClampMagnitude(local / _radius, 1f);

            if (_handle != null)
            {
                _handle.anchoredPosition = value * _radius;
            }
            _inputSender?.RouteMove(value);
        }
    }
}
