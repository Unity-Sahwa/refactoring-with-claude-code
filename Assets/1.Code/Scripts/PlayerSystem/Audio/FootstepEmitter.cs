using UnityEngine;

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

    // 걷기 효과음을 재생시키는 Footstep()을 애니메이션 이벤트를 통해 호출.
    // 대원_TODO: 모바일 이동 입력이 MoveX, MoveY에 어떻게 들어오는지 확인필요
    [RequireComponent(typeof(Animator))]
    public class FootstepEmitter : MonoBehaviour
    {
        [Inject] private AudioChannel channel;
        [SerializeField] private AudioId footstepId;
        [SerializeField] private float moveThreshold = 0.5f; // 이 미만이면 무시
        private Animator _animator;

        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");

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

        private void Awake() 
        {
            _animator = GetComponent<Animator>();
        }

        // 각 방향 클립의 발 이벤트가 자기 WalkType을 담아 호출한다.
        public void Footstep(WalkType type)
        {
            if (channel == null || _animator == null) return;
            if (!TryGetMoveDirection(out WalkType moving)) return; // 정지(입력 ~0)면 방향 없음 → 무시
            if (moving != type) return;                            // 지금 이동 방향 클립의 이벤트만 통과

            channel.RaisePlay(AudioPlayRequest.At(footstepId, transform.position));
        }

        // 이동 입력(MoveX/MoveY)이 임계값 이상이면 방향을 돌려준다.
        private bool TryGetMoveDirection(out WalkType direction)
        {
            float x = _animator.GetFloat(MoveXHash);
            float y = _animator.GetFloat(MoveYHash);

            direction = default;
            if (x * x + y * y < moveThreshold * moveThreshold) return false; // 이동 크기가 임계값보다 작으면 false 반환

            // 대원_STUDY: 각도와 라디안 사전지식
            // 둘레/지름이 항상 일정하다는 발견! 둘레/지름 = π 라 지음. 둘레 2πr
            // 호의 길이(s)가 반지름(r)과 똑같아지는 순간의 벌어진 각도 = 1 라디안
            // 그럼 한바퀴을 가정했을 때, 2πr/r = 2π 라디안. 360도를 라디안으로 계산하면 2π 라디안.
            
            // Atan2는 라디안을 반환하고, 거기에 180 / π 라디안을 곱해주어 각도로 변환시킴
            // 8방향으로 걷기 애니메이션이 존재하기에 좌표보다는 각도로 구분
            float angle = Mathf.Atan2(x, y) * Mathf.Rad2Deg; 
            if (angle < 0f) angle += 360f;
            direction = OctantToType[Mathf.RoundToInt(angle / 45f) % 8]; //반올림해서 나온 int가 방향 enum을 가리킴
            return true;
        }
    }
}
