using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 씬의 PlatformObject를 전부 모아 플랫폼에 맞춰 한 번에 켜고 끈다.
    // 설정으로 고르는 게 아니라 빌드한 플랫폼을 그대로 따라간다.
    public class PlatformController : MonoBehaviour
    {
        [Preserve, Inject(true)] private List<PlatformObject> _objects;

        // 모바일에선 마우스/터치로 카메라를 돌리면 안 되므로 Look의 포인터 바인딩만 껐다 켠다.
        // 이 에셋은 InputSystem 소유가 아니라 DataContainer에 등록된 유니티 패키지 SO다.
        // 그래서 InputSystem을 거치지 않고 여기서 직접 받아 쓴다.
        [Preserve, Inject(true)] private InputActionAsset _actionAsset;

        // 카메라 쪽 회전은 Cinemachine 축이 따로 물고 있어서, 그 주인에게 꺼달라고 시킨다.
        [Preserve, Inject(true)] private IPointerLookControl _pointerLook;

        [SerializeField] private int mobileTargetFPS = 60;
        [SerializeField] private int windowTargetFPS = 60;
        private const string LookAction = "Look";
        private const string MousePath = "<Mouse>/delta";

        // ponytail: 주입이 Awake에 끝나므로 Start에서 한 번만 맞춘다.
        private void Start()
        {
            Apply();
        }

        private void Apply()
        {
            bool isMobile = Application.isMobilePlatform;

            Application.targetFrameRate = isMobile ? mobileTargetFPS : windowTargetFPS;

            ApplyLookBinding(isMobile);

            // 셋 다 없어도 게임은 돌아간다. 다만 조용히 넘기면 "왜 안 꺼지지"를 씬에서 찾게 되므로 남긴다.
            if (_pointerLook == null)
            {
                Debug.LogWarning($"{name}: 카메라 회전 창구를 못 찾음. 모바일에서 화면을 문지르면 카메라가 같이 돈다.");
            }
            else
            {
                _pointerLook.SetPointerLookEnabled(!isMobile);
            }

            if (_objects == null)
            {
                Debug.LogWarning($"{name}: PlatformObject를 하나도 못 찾음. 플랫폼별 오브젝트가 전부 그대로 남는다.");
                return;
            }

            foreach (PlatformObject obj in _objects)
            {
                obj.gameObject.SetActive(obj.IsMobileOnly == isMobile);
            }
        }

        // 빈 문자열이면 그 바인딩이 꺼지고, null이면 원래 경로로 되돌아간다.
        private void ApplyLookBinding(bool isMobile)
        {
            InputAction look = _actionAsset?.FindAction(LookAction);

            if (look == null)
            {
                Debug.LogWarning($"{name}: 액션 에셋의 '{LookAction}'을 못 찾음. 모바일에서 마우스로 카메라가 돈다.");
                return;
            }

            look.ApplyBindingOverride(isMobile ? "" : null, path: MousePath);
        }
    }
}
