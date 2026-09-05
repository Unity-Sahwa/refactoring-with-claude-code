using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 카메라와 플레이어 사이를 가리는 오브젝트에 구멍을 뚫도록 셰이더에 기준값을 넘긴다.
    // 흐름: 현재 캐릭터의 몸 중앙 계산 → 가림 판정 → 화면 좌표로 변환 → 전역 셰이더 값 전달
    public class SeeThroughWall : MonoBehaviour
    {
        // static readonly인 이유: 셰이더 프로퍼티 id는 셰이더가 정한 값이라 인스턴스마다 달라지지 않는다.
        // 머티리얼이 아니라 전역 값을 쓰는 이유: 머티리얼에 직접 넣으면 나무 종류가 늘어날 때
        // 슬롯 하나만 갱신되고 나머지는 각자 저장된 옛 값으로 제각각 동작한다.
        // 셰이더 그래프 프로퍼티는 머티리얼 값이 전역 값을 이기므로, SeeThroughGlobals.hlsl에
        // 전역 변수를 직접 선언하고 Custom Function 노드로 읽는다.
        private static readonly int PositionId = Shader.PropertyToID("_SeeThroughPosition");
        private static readonly int SizeId = Shader.PropertyToID("_SeeThroughSize");
        private static readonly int OpacityId = Shader.PropertyToID("_SeeThroughOpacity");
        private static readonly int EdgeSoftnessId = Shader.PropertyToID("_SeeThroughEdgeSoftness");

        [Tooltip("구멍 반경")]
        [SerializeField, Range(0f, 3f)] private float _holeSize = 0.2f;

        [Tooltip("구멍 안쪽에 남는 진하기. 0이면 완전히 뚫린다")]
        [SerializeField, Range(0f, 1f)] private float _opacity;

        [Tooltip("구멍 경계가 퍼지는 정도")]
        [SerializeField, Range(0f, 1f)] private float _edgeSoftness = 0.5f;

        [Tooltip("가림 판정에 쓸 레이어. 벽 등 가리는 오브젝트만 포함")]
        [SerializeField] private LayerMask _occluderMask;

        [Preserve, Inject] private ICurrentCharacterProvider _currentCharacter;

        private Camera _camera;
        private CharacterController _controller;
        private Transform _cachedCharacterTransform;

        private void Awake()
        {
            _camera = Camera.main;
        }

        // 시네머신이 LateUpdate에서 카메라를 옮기므로, 그 뒤 위치로 계산한다.
        private void LateUpdate()
        {
            Transform characterTransform = _currentCharacter?.GetCurrentComponent<Transform>();

            if (characterTransform == null || _camera == null)
            {
                return;
            }

            if (characterTransform != _cachedCharacterTransform)
            {
                _cachedCharacterTransform = characterTransform;
                _controller = _currentCharacter.GetCurrentComponent<CharacterController>();
            }

            Vector3 bodyCenter = GetBodyCenter(characterTransform);

            // 카메라와 몸 중앙 사이에 실제로 가리는 오브젝트가 있을 때만 구멍을 뚫는다.
            if (!IsOccluded(bodyCenter))
            {
                Shader.SetGlobalFloat(SizeId, 0f);
                return;
            }

            // z에 카메라~플레이어 거리가 들어가므로, 그보다 가까운 픽셀만 셰이더가 깎는다.
            // 전역 값은 1비트뿐이라 레이캐스트로 판정하면 화면 안 모든 대상이 같이 뚫린다.
            Vector3 view = _camera.WorldToViewportPoint(bodyCenter);

            Shader.SetGlobalFloat(SizeId, _holeSize);
            Shader.SetGlobalFloat(OpacityId, _opacity);
            Shader.SetGlobalFloat(EdgeSoftnessId, _edgeSoftness);
            Shader.SetGlobalVector(PositionId, view);
        }

        private bool IsOccluded(Vector3 bodyCenter)
        {
            Vector3 origin = _camera.transform.position;
            Vector3 offset = bodyCenter - origin;
            float distance = offset.magnitude;

            return Physics.Raycast(origin, offset / distance, distance, _occluderMask);
        }

        // 캐릭터마다 스케일이 달라, 콜라이더 중앙을 월드 좌표로 바꿔서 쓴다.
        private Vector3 GetBodyCenter(Transform characterTransform)
        {
            if (_controller == null)
            {
                return characterTransform.position;
            }

            return characterTransform.TransformPoint(_controller.center);
        }
    }
}
