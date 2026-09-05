using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 카메라·락온 상태를 화면 왼쪽 위에 글자로 띄워 개발 중 확인하게 한다.
    public class CameraDebugHud : MonoBehaviour
    {
        [Preserve, Inject(true)] private ILockOnState _lockOn;
        [Preserve, Inject(true)] private ILockOnTarget _lockOnTarget;
        [Preserve, Inject(true)] private ILockOnTargetDetector _detector;

        private CinemachineBrain _brain;
        private GUIStyle _style;

        private void Awake()
        {
            if (Camera.main != null)
            {
                _brain = Camera.main.GetComponent<CinemachineBrain>();
            }
        }

        private void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 20, normal = { textColor = Color.white } };

            string activeCamera = _brain != null && _brain.ActiveVirtualCamera != null
                ? _brain.ActiveVirtualCamera.Name
                : "(없음)";
            bool isLockOn = _lockOn != null && _lockOn.IsLockOn;
            string locked = _lockOnTarget != null && _lockOnTarget.LockedTarget != null ? _lockOnTarget.LockedTarget.name : "-";
            string detected = _detector != null ? _detector.Candidates.Count.ToString() : "-";

            // 밝은 씬에서도 글자가 보이도록 어두운 판을 먼저 깐다.
            GUI.Box(new Rect(8, 8, 320, 128), GUIContent.none);

            GUI.Label(new Rect(16, 14, 600, 28), $"활성 카메라: {activeCamera}", _style);
            GUI.Label(new Rect(16, 42, 600, 28), $"IsLockOn: {isLockOn}", _style);
            GUI.Label(new Rect(16, 70, 600, 28), $"락온 대상: {locked}", _style);
            GUI.Label(new Rect(16, 98, 600, 28), $"탐지 후보: {detected}개", _style);
        }
    }
}
