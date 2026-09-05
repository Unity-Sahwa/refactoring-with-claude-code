using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace Refactoring
{
    public enum WalkType
    {
        Front,
        FrontLeft,
        Left,
        BackLeft,
        Back,
        BackRight,
        Right,
        FrontRight,
    }

    // 책임: 걷기 애니메이션 이벤트가 부른 방향과 실제 이동 방향이 같을 때만 발소리를 재생한다.
    [RequireComponent(typeof(Animator))]
    public class FootstepEmitter : MonoBehaviour
    {
        [Preserve, Inject] private AudioChannel _channel;
        // 이동 방향은 Animator 파라미터를 훔쳐보지 않고 값으로 받는다.
        [Preserve, Inject] private IMoveDirectionProvider _moveDirectionProvider;
        [SerializeField] private SoundType _footstepId;
        [Tooltip("이동 입력 크기가 이 값 미만이면 발소리를 내지 않는다")]
        [SerializeField] private float _minMoveInput = 0.5f;

        private static readonly WalkType[] OctantToType =
        {
            WalkType.Front,      //   0° ( 0, +1) 앞
            WalkType.FrontRight, //  45° (+1, +1) 앞오른
            WalkType.Right,      //  90° (+1,  0) 오른
            WalkType.BackRight,  // 135° (+1, -1) 뒤오른
            WalkType.Back,       // 180° ( 0, -1) 뒤
            WalkType.BackLeft,   // 225° (-1, -1) 뒤왼
            WalkType.Left,       // 270° (-1,  0) 왼
            WalkType.FrontLeft,  // 315° (-1, +1) 앞왼
        };

        // 매 호출마다 조용히 참지 않고, 시작할 때 한 번 알리고 꺼진다.
        private void Awake()
        {
            if (_channel == null || _moveDirectionProvider == null)
            {
                Debug.LogError($"[{nameof(FootstepEmitter)}] AudioChannel 또는 IMoveDirectionProvider가 없어 발소리를 끈다.", this);
                enabled = false;
            }
        }

        // 각 방향 클립의 발 이벤트가 자기 WalkType을 담아 호출한다.
        public void Footstep(WalkType type)
        {
            // 애니메이션 이벤트는 컴포넌트가 꺼져 있어도 들어오므로 여기서 막는다.
            if (!enabled)
            {
                return;
            }
            // 정지(입력 ~0)면 방향 없음 → 무시
            if (!TryGetMoveDirection(out WalkType moving))
            {
                return;
            }
            // 지금 이동 방향 클립의 이벤트만 통과
            if (moving != type)
            {
                return;
            }

            _channel.RaisePlay(AudioPlayRequest.CreateAt(_footstepId, transform.position));
        }

        // 이동 방향 크기가 임계값 이상이면 8방향 중 하나를 돌려준다.
        private bool TryGetMoveDirection(out WalkType direction)
        {
            Vector2 move = _moveDirectionProvider.MoveDirection;
            float x = move.x;
            float y = move.y;

            direction = default;
            if (x * x + y * y < _minMoveInput * _minMoveInput)
            {
                return false;
            }

            // 8방향 걷기 애니메이션이라 좌표가 아니라 각도로 구분한다.
            float angle = Mathf.Atan2(x, y) * Mathf.Rad2Deg; 
            if (angle < 0f)
            {
                angle += 360f;
            }
            // 반올림해서 나온 int가 8방향 enum의 인덱스가 된다.
            direction = OctantToType[Mathf.RoundToInt(angle / 45f) % 8];
            return true;
        }
    }
}
