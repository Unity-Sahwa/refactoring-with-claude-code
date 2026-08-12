using UnityEngine;

namespace Refactoring
{
    // 역할: 카메라와 플레이어 사이를 가리는 오브젝트에 구멍을 뚫도록, 셰이더에 기준값을 넘긴다.
    // 흐름: 현재 캐릭터의 몸 중앙을 구해 화면 좌표로 바꾸고, 구멍 설정과 함께 전역 셰이더 값으로 보낸다.
    //
    // 여기까지 온 이유 (같은 실수를 반복하지 않기 위해 남김):
    //
    // 1) 머티리얼에 직접 SetFloat 하던 방식은 나무 종류가 늘어나면 못 쓴다.
    //    슬롯이 하나뿐이라 나머지 머티리얼은 갱신되지 않고, 각자 저장된 옛 값으로 제각각 동작한다.
    //
    // 2) 그렇다고 Shader.SetGlobal로 바꾸는 것만으로는 해결되지 않는다.
    //    셰이더 그래프 프로퍼티는 Show In Inspector를 꺼도 머티리얼이 값을 계속 소유해서,
    //    머티리얼 값이 전역 값을 이긴다. 그래서 SeeThroughGlobals.hlsl에 전역 변수를 직접 선언하고
    //    Custom Function 노드로 읽는다. 머티리얼이 개입할 방법이 없어진다.
    //
    // 3) 처음에는 Physics.Raycast로 "가려졌나"를 판정했지만, 전역 값은 1비트뿐이라
    //    어느 오브젝트가 가렸는지 구분하지 못한다. 하나라도 가리면 화면 안의 모든 대상이 같이 뚫렸다.
    //    지금은 셰이더가 픽셀 깊이를 플레이어 깊이와 비교해 스스로 판정하므로 레이캐스트가 필요 없다.
    public class SeeThroughWall : MonoBehaviour
    {
        private static readonly int PositionId = Shader.PropertyToID("_SeeThroughPosition");
        private static readonly int SizeId = Shader.PropertyToID("_SeeThroughSize");
        private static readonly int OpacityId = Shader.PropertyToID("_SeeThroughOpacity");
        private static readonly int EdgeSoftnessId = Shader.PropertyToID("_SeeThroughEdgeSoftness");

        [Inject] private ICurrentCharacterProvider _currentCharacter;

        [Tooltip("구멍 반경")]
        [SerializeField, Range(0f, 3f)]
        private float _holeSize = 0.2f;

        [Tooltip("구멍 안쪽에 남는 진하기. 0이면 완전히 뚫린다")]
        [SerializeField, Range(0f, 1f)]
        private float _opacity;

        [Tooltip("구멍 경계가 퍼지는 정도")]
        [SerializeField, Range(0f, 1f)]
        private float _edgeSoftness = 0.5f;

        private Camera _camera;
        private CharacterController _controller;
        private PlayerCharacter _cachedCharacter;

        private void Awake()
        {
            _camera = Camera.main;
        }

        // 시네머신이 LateUpdate에서 카메라를 옮기므로, 그 뒤 위치로 계산한다.
        private void LateUpdate()
        {
            PlayerCharacter character = _currentCharacter?.CurrentCharacter;

            if (character == null || _camera == null)
            {
                return;
            }

            if (character != _cachedCharacter)
            {
                _cachedCharacter = character;
                _controller = character.GetComponent<CharacterController>();
            }

            // z에 카메라~플레이어 거리가 들어가므로, 그보다 가까운 픽셀만 셰이더가 깎는다.
            Vector3 view = _camera.WorldToViewportPoint(GetBodyCenter(character));

            Shader.SetGlobalFloat(SizeId, _holeSize);
            Shader.SetGlobalFloat(OpacityId, _opacity);
            Shader.SetGlobalFloat(EdgeSoftnessId, _edgeSoftness);
            Shader.SetGlobalVector(PositionId, view);
        }

        // 캐릭터마다 스케일이 달라, 콜라이더 중앙을 월드 좌표로 바꿔서 쓴다.
        private Vector3 GetBodyCenter(PlayerCharacter character)
        {
            if (_controller == null)
            {
                return character.transform.position;
            }

            return character.transform.TransformPoint(_controller.center);
        }
    }
}
