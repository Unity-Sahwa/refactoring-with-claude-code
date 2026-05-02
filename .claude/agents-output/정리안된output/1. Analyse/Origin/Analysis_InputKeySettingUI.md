# InputKeySettingUI 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | InputKeySettingUI |
| 현재 역할 | 입력 키 설정 UI 관리<br>- 플레이어 입력 키 바인딩 변경<br>- 저장된 키 설정 로드/저장<br>- 키 입력 감지 및 UI 업데이트 |
| 구현 디자인 패턴 | MonoBehaviour (UI 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 버튼 배열 초기화 | 1. inputKeyButtonText 배열 크기를 (int)KeyAction.KEYCOUNT로 설정 | KeyAction |
| 버튼 이벤트 등록 | 1. 모든 inputKeyButton에 대해:<br>   - 클로저 문제 해결을 위해 int index = i로 로컬 변수 생성<br>   - onClick.AddListener()로 EditInputKey(index) 콜백 등록<br>2. 버튼마다 GetComponentInChildren<TextMeshProUGUI>()로 텍스트 컴포넌트 캐싱 | Button <br>TextMeshProUGUI |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| SaveManager 인스턴스 획득 | 1. SaveManager.instance에서 saveManager 획득 | SaveManager |
| 저장된 키 설정 로드 | 1. 모든 KeyAction에 대해 ChangeInputKeyButtonText(i) 호출로 저장된 키 표시 | ChangeInputKeyButtonText() |

### Update()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 키 편집 모드 진행 | 1. isEditingKey가 true이면:<br>   - DetectPressedKeyCode() 호출로 입력된 키 감지<br>   - 반환값이 KeyCode.None이 아니면 키 설정 진행 | DetectPressedKeyCode() |
| 메뉴 버튼 마우스 클릭 필터링 | 1. currentKeyIndex == (int)KeyAction.MENU이고 감지된 키가 Mouse0이면:<br>   - 메뉴 버튼 클릭으로 인한 중복 입력 방지하고 반환 | KeyAction |
| 입력 키 저장 | 1. saveManager.ChangeKeySetting(currentKeyIndex, DetectPressedKeyCode()) 호출<br>2. saveManager.SaveInputKeyData() 호출로 파일에 저장 | SaveManager |
| UI 업데이트 | 1. ChangeInputKeyButtonText(currentKeyIndex) 호출로 버튼 텍스트 변경 | ChangeInputKeyButtonText() |
| 편집 상태 종료 | 1. currentKeyIndex = -1 설정<br>2. isEditingKey = false 설정<br>3. completeEditingKey = true 설정 | - |

### CompleteEditingKey(bool complete)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 편집 완료 플래그 설정 | 1. completeEditingKey = complete로 외부에서 상태 제어 가능 | - |

### DetectPressedKeyCode()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 현재 프레임 입력 키 감지 | 1. Enum.GetValues(typeof(KeyCode)) 루프를 통해 모든 KeyCode 순회<br>2. Input.GetKeyDown(kcode)로 눌린 키 감지<br>3. 첫 눌린 키의 KeyCode 반환<br>4. 눌린 키 없으면 KeyCode.None 반환 | Input |

### EditInputKey(int index)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 키 편집 모드 활성화 | 1. isEditingKey = true로 설정<br>2. currentKeyIndex = index로 편집 대상 키 저장<br>3. Update에서 키 입력 감지 시작 | - |

### ChangeInputKeyButtonText(int index)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력 키 버튼 텍스트 갱신 | 1. saveManager.InputKeys 딕셔너리에서 (KeyAction)index 조회<br>2. 해당 키 존재 여부 확인<br>3. 존재하면:<br>   - saveManager.InputKeys[(KeyAction)index]의 KeyCode 획득<br>   - KeyCode를 .ToString()으로 사람이 읽을 수 있는 텍스트로 변환<br>   - inputKeyButtonText[index].text에 설정<br>4. 설정 UI 화면에 현재 키 바인딩 즉시 표시<br>5. 키 재설정 가능 상태로 복귀 | SaveManager <br>TextMeshProUGUI <br>KeyCode |
