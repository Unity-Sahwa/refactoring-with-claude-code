# MouseSettingUI 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | MouseSettingUI |
| 현재 역할 | 마우스 설정 UI 관리<br>- X/Y축 마우스 속도 슬라이더 제어<br>- 마우스 설정 저장/로드<br>- CameraController와의 연동 |
| 구현 디자인 패턴 | MonoBehaviour (UI 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 슬라이더 리스너 등록 | 1. mouseSpeedWithXAxisSlider.onValueChanged에 SetXAxisValue() 등록<br>2. mouseSpeedWithYAxisSlider.onValueChanged에 SetYAxisValue() 등록 | Slider |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 마우스 데이터 로드 | 1. LoadMouseData() 호출로 저장된 마우스 설정 불러오기 | LoadMouseData() |

### SetXAxisValue(float value)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| X축 값 정수 변환 | 1. value를 int로 변환 | - |
| X축 텍스트 업데이트 | 1. MouseSpeedWithXAxisText.text에 정수값 설정<br>   (슬라이더가 변경될 때마다 호출됨) | TextMeshProUGUI |
| 카메라 X축 속도 설정 | 1. cameraController.SetMouseSpeed(true, value) 호출로 카메라에 설정 | CameraController |

### SetYAxisValue(float value)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| Y축 값 정수 변환 | 1. value를 int로 변환 | - |
| Y축 텍스트 업데이트 | 1. MouseSpeedWithYAxisText.text에 정수값 설정<br>   (슬라이더가 변경될 때마다 호출됨) | TextMeshProUGUI |
| 카메라 Y축 속도 설정 | 1. cameraController.SetMouseSpeed(false, value) 호출로 카메라에 설정 | CameraController |

### SaveMouseData()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 카메라에서 현재 마우스 속도 조회 | 1. cameraController.GetMouseSpeed(true)로 X축 속도 획득<br>2. cameraController.GetMouseSpeed(false)로 Y축 속도 획득 | CameraController |
| SaveManager에 설정 저장 | 1. SaveManager.instance.ChangeMouseSetting()으로 설정 변경<br>2. SaveManager.instance.SaveMouseData()로 파일에 저장 | SaveManager |

### LoadMouseData()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| SaveManager에서 마우스 속도 로드 | 1. SaveManager.instance.mouseSpeedWithXAxis 획득<br>2. SaveManager.instance.mouseSpeedWithYAxis 획득 | SaveManager |
| 카메라 속도 설정 | 1. SetXAxisValue()로 X축 값 설정<br>2. SetYAxisValue()로 Y축 값 설정 | SetXAxisValue() <br>SetYAxisValue() |
| 슬라이더 값 동기화 | 1. mouseSpeedWithXAxisSlider.value에 저장된 X축값 설정<br>2. mouseSpeedWithYAxisSlider.value에 저장된 Y축값 설정 | Slider |
| UI 텍스트 업데이트 | 1. MouseSpeedWithXAxisText에 정수 형태로 X축값 표시<br>2. MouseSpeedWithYAxisText에 정수 형태로 Y축값 표시 | TextMeshProUGUI |
