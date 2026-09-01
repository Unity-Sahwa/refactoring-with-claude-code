using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 모바일 UI, PC/모바일 튜토리얼 가이드처럼 플랫폼에 따라 켜고 꺼질 오브젝트에 붙인다.
    // 설정창은 값만 바꾸고 대상이 뭔지 모른다. 대상이 알아서 값을 보고 자기를 켜고 끈다.
    public class PlatformToggle : MonoBehaviour
    {
        // 켜는 조건. 체크하면 모바일일 때만, 안 하면 PC일 때만 켠다.
        [SerializeField]
        private bool _showOnMobile;

        [Preserve, Inject(true)] private IPlatformSettings _settings;

        // ponytail: 켜고 끄는 판단을 매 프레임 안 하고 시작할 때 한 번만 한다.
        // 게임 중에 플랫폼을 바꾸는 경우가 생기면 그때 설정 쪽에 값 바뀜 알림을 붙이면 된다.
        private void Start()
        {
            if (_settings == null)
            {
                return;
            }

            gameObject.SetActive(_settings.IsMobile == _showOnMobile);
        }
    }
}
