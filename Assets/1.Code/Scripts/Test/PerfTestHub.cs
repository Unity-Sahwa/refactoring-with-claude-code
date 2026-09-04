using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Refactoring
{
    // 실기기 성능 확인용 최소 HUD. 1초마다 평균 FPS/ms만 갱신해서 보여준다.
    public class PerfTestHub : MonoBehaviour
    {
        public static PerfTestHub Instance;
        private const float UpdateIntervalSeconds = 1f;
        private const float LowFpsThreshold = 40f; // 이 미만이면 빨간 글씨

        // 빌드 세팅에 등록된 씬 이름 순서(Scene0~Scene5)
        private static readonly string[] SceneNames =
        {
            "Scene0_MainMenu", "Scene1_Tutorial", "Scene2", "Scene3", "Scene4", "Scene5_Boss"
        };

        private float _accumulatedSeconds;
        private int _accumulatedFrames;
        private float _averageFrameMs;
        private float _averageFps;

        private string _thermalText = "?";
        private string _thermalSysfsPath;
        private bool _thermalSysfsChecked;
        private GUIStyle _fpsLabelStyle;
        private GUIStyle _sceneButtonStyle;
        private bool _isSceneMenuOpen;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(this);
                }
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            _accumulatedSeconds += Time.unscaledDeltaTime;
            _accumulatedFrames++;

            if (_accumulatedSeconds >= UpdateIntervalSeconds)
            {
                _averageFrameMs = _accumulatedSeconds * 1000f / _accumulatedFrames;
                _averageFps = _accumulatedFrames / _accumulatedSeconds;

                _accumulatedSeconds = 0f;
                _accumulatedFrames = 0;
                _thermalText = ReadThermal();
            }
        }

        // ponytail: 안드로이드는 실제 °C 공개 API가 없음. sysfs 읽히면 °C, 막히면 getThermalHeadroom(API30+) 폴백.
        private string ReadThermal()
        {
            if (!_thermalSysfsChecked)
            {
                _thermalSysfsChecked = true;
                for (int i = 0; i < 20; i++)
                {
                    string path = $"/sys/class/thermal/thermal_zone{i}/temp";
                    try
                    {
                        if (File.Exists(path) && ParseMilliCelsius(File.ReadAllText(path)) > 0f)
                        {
                            _thermalSysfsPath = path;
                            break;
                        }
                    }
                    catch { }
                }
            }

            if (_thermalSysfsPath != null)
            {
                try
                {
                    return $"{ParseMilliCelsius(File.ReadAllText(_thermalSysfsPath)):F1}C";
                }
                catch { _thermalSysfsPath = null; }
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var power = activity.Call<AndroidJavaObject>("getSystemService", "power"))
                {
                    float headroom = power.Call<float>("getThermalHeadroom", 0);
                    int status = power.Call<int>("getCurrentThermalStatus");
                    return $"HR {headroom:F2}/S{status}";
                }
            }
            catch { }
#endif
            return "temp n/a";
        }

        private static float ParseMilliCelsius(string raw)
        {
            float value;
            if (!float.TryParse(raw.Trim(), out value))
            {
                return 0f;
            }
            return value > 1000f ? value / 1000f : value;
        }

        private void OnGUI()
        {
            if (_fpsLabelStyle == null)
            {
                _fpsLabelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 42, // 기본 라벨(14) 대비 3배
                    fontStyle = FontStyle.Bold
                };
                _sceneButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 42 // 기본 버튼 글자(14) 대비 3배
                };
            }

            const float buttonWidth = 360f; // 기존 120의 3배
            const float buttonHeight = 108f; // 기존 36의 3배
            const float spacing = 10f;
            const float toggleWidth = 80f;

            string fpsText = $"FPS {_averageFps:F0} ({_averageFrameMs:F1}ms) {Screen.width}x{Screen.height} {_thermalText}";
            Vector2 textSize = _fpsLabelStyle.CalcSize(new GUIContent(fpsText));
            var labelRect = new Rect(60, Screen.height - textSize.y - 78, textSize.x + 12, textSize.y + 8);
            GUI.color = Color.black;
            GUI.DrawTexture(labelRect, Texture2D.whiteTexture);

            bool isLow = _averageFps < LowFpsThreshold && _averageFps > 0f;
            GUI.color = isLow ? Color.red : Color.white;
            GUI.Label(labelRect, fpsText, _fpsLabelStyle);
            GUI.color = Color.white;

            var toggleRect = new Rect(labelRect.xMax + spacing, labelRect.y, toggleWidth, labelRect.height);
            if (GUI.Button(toggleRect, _isSceneMenuOpen ? "▼" : "▶", _sceneButtonStyle))
            {
                _isSceneMenuOpen = !_isSceneMenuOpen;
            }

            if (_isSceneMenuOpen)
            {
                for (int i = 0; i < SceneNames.Length; i++)
                {
                    float y = toggleRect.y - (i + 1) * (buttonHeight + spacing);
                    if (GUI.Button(new Rect(toggleRect.x, y, buttonWidth, buttonHeight), SceneNames[i], _sceneButtonStyle))
                    {
                        SceneManager.LoadScene(SceneNames[i]);
                        _isSceneMenuOpen = false;
                    }
                }
            }
        }
    }
}
