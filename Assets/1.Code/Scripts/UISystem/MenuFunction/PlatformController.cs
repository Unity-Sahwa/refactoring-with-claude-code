using System.Collections.Generic;
using Unity.Cinemachine;
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
        [Preserve, Inject(true)] private InputActionAsset _actionAsset;
        [Preserve, Inject(true)] private CinemachineInputAxisController _cameraAxisController;

        [SerializeField] private int mobileTargetFPS = 60;
        [SerializeField] private int windowTargetFPS = 60;
        private const string LookAction = "Look";
        private const string MousePath = "<Mouse>/delta";

        // 카메라는 PlayerInputActions가 아니라 CinemachineInputAxisController에 물린 자기 액션을 쓰고,
        // 그 액션은 Pointer/delta라 마우스뿐 아니라 터치도 그대로 잡는다. 그래서 따로 꺼준다.
        private const string LookOrbitX = "Look Orbit X";
        private const string LookOrbitY = "Look Orbit Y";
        private const string PointerDeltaPath = "<Pointer>/delta";

        // ponytail: 주입이 Awake에 끝나므로 Start에서 한 번만 맞춘다.
        private void Start()
        {
            Apply();
        }

        private void Apply()
        {
            bool isMobile = Application.isMobilePlatform;

            // 모바일은 발열/스로틀링 때문에 40프레임으로 제한한다.
            Application.targetFrameRate = isMobile ? mobileTargetFPS : windowTargetFPS;

            // 빈 문자열이면 그 바인딩이 꺼지고, null이면 원래 경로로 되돌아간다.
            _actionAsset?.FindAction(LookAction)?.ApplyBindingOverride(isMobile ? "" : null, path: MousePath);

            _cameraAxisController?.GetController(LookOrbitX)?.Input?.InputAction?.action
                ?.ApplyBindingOverride(isMobile ? "" : null, path: PointerDeltaPath);
            _cameraAxisController?.GetController(LookOrbitY)?.Input?.InputAction?.action
                ?.ApplyBindingOverride(isMobile ? "" : null, path: PointerDeltaPath);

            if (_objects == null)
            {
                return;
            }

            foreach (PlatformObject obj in _objects)
            {
                obj.gameObject.SetActive(obj.IsMobileOnly == isMobile);
            }
        }
    }
}
