using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class MouseSettingUI : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    
    [SerializeField] private Slider mouseSpeedWithXAxisSlider;
    [SerializeField] private Slider mouseSpeedWithYAxisSlider;
    [SerializeField] private TextMeshProUGUI MouseSpeedWithXAxisText;
    [SerializeField] private TextMeshProUGUI MouseSpeedWithYAxisText;

    private void Awake()
    {
        mouseSpeedWithXAxisSlider.onValueChanged.AddListener(SetXAxisValue);
        mouseSpeedWithYAxisSlider.onValueChanged.AddListener(SetYAxisValue);
    }
    private void Start()
    {
        LoadMouseData();
    }


    public void SetXAxisValue(float value)
    {//슬라이더에 연동되는 함수

        //카메라에 value 전달
        int intValue = (int)value;
        MouseSpeedWithXAxisText.text = intValue.ToString();
        cameraController.SetMouseSpeed(true, value);
    }
    public void SetYAxisValue(float value)
    {//슬라이더에 연동되는 함수
        
        int intValue = (int)value;
        MouseSpeedWithYAxisText.text = intValue.ToString();
        cameraController.SetMouseSpeed(false, value);
    }

    public void SaveMouseData()
    {
        //현재 디폴트카메라에 적용된 수치 저장
        float mouseSpeedOfXAxis = cameraController.GetMouseSpeed(true);
        float mouseSpeedOfYAxis = cameraController.GetMouseSpeed(false);

        SaveManager.instance.ChangeMouseSetting(mouseSpeedOfXAxis, mouseSpeedOfYAxis);
        SaveManager.instance.SaveMouseData();
    }

    public void LoadMouseData()
    {//saveManager 스크립트의 마우스속도 수치 불러오기(파일의 값을 불러오진 않음)

        //게임 시작시, SaveManager에서 파일의 수치를 불러옴
        //그 후, 해당 수치를 불러옴.
        //TODO: TimelineHelper에서 savemanager 불러오기 실패함 -> instance으로 일단진행

        //카메라에 먼저 수치를 보냄
        SetXAxisValue(SaveManager.instance.mouseSpeedWithXAxis);
        SetYAxisValue(SaveManager.instance.mouseSpeedWithYAxis);

        //그 수치를 슬라이더 값에 가져옴
        mouseSpeedWithXAxisSlider.value = SaveManager.instance.mouseSpeedWithXAxis;
        mouseSpeedWithYAxisSlider.value = SaveManager.instance.mouseSpeedWithYAxis;

        int intXValue = (int)SaveManager.instance.mouseSpeedWithXAxis;
        int intYValue = (int)SaveManager.instance.mouseSpeedWithYAxis;
        MouseSpeedWithXAxisText.text = intXValue.ToString();
        MouseSpeedWithYAxisText.text = intYValue.ToString();
    }
}
