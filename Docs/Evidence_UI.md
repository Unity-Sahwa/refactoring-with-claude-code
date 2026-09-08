# 리팩토링 근거 — UI 시스템 (레거시 대비 실측)

**주제: 판단 코드를 자료구조로 대체하기.** 뒤로가기 if 5단 중첩을 없앨 것이 아니라, 스택이 순서를 기억하게 함.

여기 적힌 것은 양쪽 코드 실측이거나 본인 답변임. 추측은 적지 않음.

비교 대상
- before: `Unitysahwa-refactoring-with-claudecode(before)/Assets/Scripts/UI` (13파일 2,212줄)
- after: `Unitysahwa-refactoring/Assets/1.Code/Scripts/UISystem` (28파일 1,380줄)

## 총량 지표 (실측)
| 지표 | before | after |
|---|---|---|
| 파일 수 | 13 | 28 |
| 총 줄 수 | 2,212 | 1,380 |
| 최대 단일 파일 | `MenuUI` 814줄 | `UIOrderViewer`(에디터 툴) 148줄 / 런타임 최대는 `UIRoot` 138줄 |
| 창 시스템 | 없음 | `UIRoot` + `UINavigator` + `IWindow` + `UIWindow` + `WindowType` |

## 핵심 대비 — 창 관리 방식

### before: `MenuUI` 814줄이 전부 담당
- 창 9개(새게임·일시정지·메인메뉴복귀·설정·입력키·슬롯목록·불러오기·종료·스토리)를
  `SetUIElements()`에서 `SetActive(false)`로 일괄 초기화. 창 추가 시 이 메서드를 고쳐야 함
- 버튼 동작을 `SetButtonFunction()`에서 반복문 + 인덱스로 `AddListener(() => ...SetActive(...))` 배선
- **뒤로가기 처리**: `MenuSwitch()`가 각 창의 `activeSelf`를 직접 물어보며 **if-else 5단 중첩**으로 닫을 대상을 결정
- `SetActive` 호출 35회. "창이 열려 있다" = GameObject가 켜져 있다와 동일. 별도 상태 개념 없음
- `SerializeField` 46개

### before: UI가 게임 전체를 알고 있었음
- `SerializeField` 외부 참조 8개: `CameraController`·`MaskChange`·`PlayerSound`·`PlayerState`·`GameTimeScale`·`SoundSettingUI`·`InputKeySettingUI`·`MouseSettingUI`
- 싱글톤 참조 6종: `TimelineHelper`·`LoadingUI`·`PlayGuide`·`SceneSwitcher`·`PlatformSwitcher`·`SaveManager`
- `MenuUI` 자신도 `public static instance` 싱글톤
- 결과: 메뉴 UI 한 클래스가 플레이어·카메라·사운드·씬·세이브·플랫폼·컷씬을 전부 앎

### after: 스택 + 계약으로 분리
| 구성 | 줄 | 책임 |
|---|---|---|
| `IWindow` | 8 | `Open()`/`Close()` 두 개만 약속 |
| `UIWindow` | 19 | 이름표(`WindowType`)를 들고 켜기/끄기만. `Close`는 virtual |
| `UINavigator` | 33 | `Stack<IWindow>`에 쌓고 맨 위부터 닫음 |
| `UIRoot` | 138 | 자식 창을 이름표로 자동 등록, 열기/닫기와 게임 모드 전환 조율 |

- **뒤로가기 순서를 판단하는 코드가 없음.** 스택이 순서를 대신함 (`CloseTop()`)
- `UIRoot.BuildRegistry()`가 `GetComponentsInChildren<UIWindow>(true)`로 자식 창을 자동 수집.
  창을 추가해도 `UIRoot`는 변경되지 않음
- 외부 의존은 선택적 주입 4개(`IInputPressedProvider`·`IMenuInputProvider`·`ICutsceneInputProvider`·`IGameStateController`), 전부 인터페이스
- 메인메뉴 씬 / 인게임 씬 차이는 `_entryWindow` 인스펙터 값 하나로 구분

## 핵심 요약
- before는 "지금 어떤 창이 열려 있나"를 GameObject 활성 상태로 되묻고 if 중첩으로 판단함
- after는 열린 순서를 스택이 기억하므로 판단 자체가 사라짐
- before는 UI가 게임 시스템을 직접 붙잡았고, after는 인터페이스 4개만 선택적으로 받음

## 세부 대비

### 확인창 (예/아니오)
| | before | after |
|---|---|---|
| 위치 | `MenuUI` 814줄 안의 분기 (`newGameWindow`·`quitWindow`·`goToMainMenuWindow`를 `activeSelf`로 판별) | `ConfirmWindow` 추상 클래스 + 파생 3종 |
| 파생 크기 | — | `NewGameConfirmWindow` 11줄 / `LoadConfirmWindow` 18줄 / `QuitConfirmWindow` 18줄 |
| 공통 처리 | 각 창마다 반복 | `ConfirmWindow`가 예 버튼 배선·창 닫기 순서를 담당, 파생은 `RunYes()`만 구현 |

확인창 추가 비용: before는 `MenuUI`에 필드 + 초기화 + 뒤로가기 분기 추가 / after는 11줄짜리 클래스 1개.

### UI 이펙트
| | before `UIEffect` (202줄) | after |
|---|---|---|
| 구조 | `public static instance` 싱글톤 | `UIFader`(59줄) / `UIShake`(78줄) |
| 대상 보관 | `CanvasGroup` 4개 + `Image` 3개 + `Coroutine` 2개를 필드로 직접 보유 | SO에서 효과 종류를 드롭다운 선택, 채널로 재생/정지 요청 |
| 효과 추가 | 전용 public 메서드 추가 (`FadeOutScreen`·`ShowDeathScreen`·`ShowBossDefeatedScreen` 등 **10개**가 이미 존재) | 데이터 항목 추가 |
| 외부 의존 | `PlayerSound` 직접 참조 | 주입 인터페이스 |

효과마다 전용 메서드와 전용 필드가 늘어나는 구조였음. 화면 종류가 늘면 클래스가 그만큼 커짐.

### 설정 UI
| | before | after |
|---|---|---|
| 구성 | `SoundSettingUI`(174) / `InputKeySettingUI`(119) / `MouseSettingUI`(76) 3파일 | `SettingsWindow`(58) + `VolumeSlider`(38) + `MouseSpeedSlider`(67) + `KeyRebindButton`(121) |
| 저장 접근 | 셋 다 `SaveManager.instance`를 직접 호출 | 주입 (`SettingsWindow` 1개, 나머지 각 1~2개) |
| 믹서 접근 | `SoundSettingUI`가 `AudioMixer` 3개를 직접 보유 | `VolumeSlider`가 볼륨 계약만 호출 |
| `MenuUI`와의 관계 | `MenuUI`가 셋 다 `SerializeField`로 물고, 셋도 `MenuUI`를 참조 | 부모-자식 관계 없이 각자 독립 |

레거시 `MouseSettingUI`에는 `// TODO: TimelineHelper에서 savemanager 불러오는 문제 -> instance부터 하는건지` 주석이 남아 있음.
싱글톤 접근 순서 문제를 인지하고 있었으나 해결하지 못한 상태였음.

### 세이브 슬롯 UI
| | before | after |
|---|---|---|
| 위치 | `MenuUI` 안 (`loadSlotWindow`·`loadWindow`를 `activeSelf`로 2단 분기, `SaveManager.instance.SelectedIndex` 직접 조작) | `SlotListWindow`(85줄) + `SaveSlotButtonView`(49) + `LoadSlotButton` + `ISaveSlots`(21) |
| 세이브 접근 | `SaveManager.instance` 직접 | `ISaveSlots` 계약을 통해 주입 |

## 정리
UI에서 바뀐 것은 화면 개수가 아니라 **책임의 위치**임.
- 창 상태 판단: GameObject `activeSelf` 조회 → 스택
- 창 등록: `MenuUI` 필드 46개 수동 배선 → `UIRoot`가 자식 자동 수집
- 시스템 접근: 싱글톤 6종 직접 호출 → 인터페이스 선택적 주입
- 기능 추가 단위: 814줄 클래스 수정 → 11~85줄 클래스 신규
