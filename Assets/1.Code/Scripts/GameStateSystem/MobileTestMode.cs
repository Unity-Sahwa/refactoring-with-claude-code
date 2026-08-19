using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Refactoring
{
    // 에디터에서 모바일용 화면을 테스트할 때 켜서 쓰는 전용 오브젝트.
    // 켜져 있으면 커서를 계속 보이게 두고, 마우스로 인한 카메라 회전만 막고(게임패드·터치는 그대로 둠),
    // 씬의 PlatformObject도 모바일 배치로 강제 전환한다.
    // PlatformController·CursorController·MouseSpeedApplier 등 실제 로직은 이 클래스의 존재를 모른다 —
    // 여기서 대상을 직접 찾아 덮어쓰는 방식이라 테스트 흔적이 그쪽에 남지 않는다.
    // 실제 모바일 기기는 커서/마우스 자체가 없어서 이 값과 무관함.
    [DefaultExecutionOrder(100)] // PlatformController가 Start에서 PC 배치로 맞춘 뒤에 덮어써야 해서 뒤로 미룸
    public class MobileTestMode : MonoBehaviour
    {
        [SerializeField] private bool isMobileTest = false;
        [Inject] private CinemachineInputAxisController cameraAxisController;
        [Inject(true)] private List<PlatformObject> _platformObjects;

        // 온스크린 조이스틱(OnScreenStick)은 드래그를 <Gamepad>/rightStick으로 흉내 내서 보낸다.
        // 그런데 에디터에서는 실제 마우스로 그 조이스틱을 드래그하므로, 같은 프레임에
        // 진짜 Mouse/delta도 같이 발생해 activeControl이 매 프레임 마우스↔가상 게임패드로 튀어
        // 프레임 단위로 막았다 풀었다 하면 버벅거린다. 그래서 프레임마다 판정하지 않고
        // <Pointer>/delta 바인딩 자체를 정적으로 껐다 켠다.
        private const string LookOrbitX = "Look Orbit X";
        private const string PointerDeltaPath = "<Pointer>/delta";

        private void Start()
        {
            if (!isMobileTest)
            {
                return;
            }

            // 마우스로 카메라를 돌리는 바인딩 자체를 지운다.
            if (cameraAxisController != null)
            {
                cameraAxisController.GetController(LookOrbitX)?.Input?.InputAction?.action
                    ?.ApplyBindingOverride("", path: PointerDeltaPath);
            }

            if (_platformObjects == null)
            {
                return;
            }

            // PlatformController가 Start에서 이미 PC 배치로 맞춰놓은 걸 여기서 덮어쓴다.
            foreach (PlatformObject obj in _platformObjects)
            {
                obj.gameObject.SetActive(obj.IsMobileOnly);
            }
        }

        private void LateUpdate()
        {
            if (!isMobileTest)
            {
                return;
            }

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
