using UnityEngine;

namespace Refactoring
{
    // 책임: 오브젝트를 시작 위치 기준으로 sin 곡선을 따라 왕복시킨다(발판, 수집품 등의 부유 연출).
    // 사용법: 트리거의 이벤트 목록에 넣으면 Execute()가 부유를 켜고 끈다(호출할 때마다 반전).
    // ponytail: 축 enum 대신 벡터 하나로 방향과 진폭을 함께 받는다. 대각선 왕복도 그대로 되고 분기가 사라진다.
    public class FloatingObject : MonoBehaviour
    {
        [Tooltip("왕복 방향과 진폭. 예: (0,0.5,0)이면 Y축으로 ±0.5 만큼 움직인다")]
        [SerializeField]
        private Vector3 _amplitude = new Vector3(0f, 0.5f, 0f);

        [Tooltip("왕복 속도")]
        [SerializeField]
        private float _speed = 1f;

        private Vector3 _startPosition;
        private float _phase;

        private void Start()
        {
            // 부모가 움직여도 기준점이 어긋나지 않도록 로컬 좌표를 쓴다.
            _startPosition = transform.localPosition;
        }

        private void Update()
        {
            // Time.time이 아니라 위상을 직접 누적한다. 껐다 켜도 멈춘 지점에서 이어져 순간이동이 없다.
            _phase += Time.deltaTime * _speed;
            transform.localPosition = _startPosition + _amplitude * Mathf.Sin(_phase);
        }
    }
}
