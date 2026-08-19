using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Refactoring
{
    // 씬의 PlatformObject를 전부 모아 플랫폼에 맞춰 한 번에 켜고 끈다.
    // 설정으로 고르는 게 아니라 빌드한 플랫폼을 그대로 따라간다.
    public class PlatformController : MonoBehaviour
    {
        [Inject(true)] private List<PlatformObject> _objects;

        // 모바일에선 마우스로 카메라를 돌리면 안 되므로 Look의 마우스 바인딩만 껐다 켠다.
        [Inject(true)] private InputActionAsset _actionAsset;
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

            // 빈 문자열이면 그 바인딩이 꺼지고, null이면 원래 경로로 되돌아간다.
            _actionAsset?.FindAction(LookAction)?.ApplyBindingOverride(isMobile ? "" : null, path: MousePath);

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
