using UnityEngine;

namespace Refactoring
{
    // F: 시작 시 화면 해상도를 1회 맞춘다. 플랫폼별 목표 픽셀 수에 기기 종횡비를 맞춰 환산한다.
    // 커서 숨김은 기존 HideCursor가 담당. 마우스 감도·플랫폼축·조이스틱 스와이프는 카메라 입력 별도 과제로 보류.
    public class ScreenSetup_Test : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;

#if UNITY_ANDROID
            int targetWidth = 1280;
            int targetHeight = 720;
#else
            int targetWidth = 1920;
            int targetHeight = 1080;
#endif
            float deviceAspect = (float)Screen.width / Screen.height;
            int targetPixelCount = targetWidth * targetHeight;

            // 목표 픽셀 수를 유지하면서 기기 종횡비에 맞는 해상도로 환산.
            float adjustedHeight = Mathf.Sqrt(targetPixelCount / deviceAspect);
            float adjustedWidth = adjustedHeight * deviceAspect;

            Screen.SetResolution((int)adjustedWidth, (int)adjustedHeight, true);
        }
    }
}
