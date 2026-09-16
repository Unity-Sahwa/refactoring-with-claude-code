# System Usage Guide (시스템 활용 가이드)

> **시스템 경계 = `Assets/1.Code/Scripts/` 바로 아래 폴더 하나.**
> **시스템 내부 기능 경계 = 시스템 폴더 바로 아래 폴더 하나**
> `Old/`, `_Test`, `PerformanceTest`, `Editor`는 대상에서 제외한다.

---

## 항목 형식

```
## <시스템명>
- 확정: <오늘 날짜, YYYY-MM-DD>
- 이 시스템이 프로젝트에서 무슨 역할을 하는지 한줄 작성

### 이 시스템을 활용하는 방법 (개발자용)
- 외부와 연결되는 클래스·인터페이스·SO를 나열함
- 클래스마다 받아쓰는 방법과 제공 기능을 표로 적음
- 제공 기능 없으면 `없음`으로 표시

### 이 시스템을 활용하는 방법 (비개발자용)
- 에디터 작업이 필요한 클래스(SO 에셋, 씬 컴포넌트 등)를 나열함
- 클래스마다 만드는 법(메뉴 경로/컴포넌트 추가), 배치 위치, 채울 수치(SO 필드 또는 SerializeField)를 적음
- DataContainer 등록이 필요한 SO면 그것도 적음
```

---

## DISystem
- 확정: 2026-09-14
- 씬과 SO에 흩어진 의존성을 어트리뷰트 하나로 찾아 필드에 꽂아줌

### 이 시스템을 활용하는 방법 (개발자용)

| 클래스 | 받아쓰는 방법 | 제공 기능 |
|---|---|---|
| `InjectAttribute` | 필드에 `[Preserve, Inject]` 또는 `[Preserve, Inject(true)]` 사용 | 없음 |
| `IDataProvider` | `IDataProvider` 구현 후 `ProvideData()` 오버라이드 | 씬 객체가 아닌 SO를 DI 대상으로 등록시킴 |

### 이 시스템을 활용하는 방법 (비개발자용)

| 클래스 | 만드는 법 | 배치 위치 | 채울 값 |
|---|---|---|---|
| `AttributeInjector` | Static 프리팹에 이미 배치됨 | 씬에 Static 프리팹 하나 | Project Settings > Script Execution Order에 -100 지정 |
| `DataContainer` | 빈 오브젝트에 컴포넌트 추가 | 씬에 하나 | `_groups`에 그룹 이름(Name)과 SO 목록(Assets) 입력 |

---

## AudioSystem
- 확정: 2026-09-14
- 재생 요청을 받아 AudioSource 풀로 소리를 내고, 설정된 볼륨값을 오디오 믹서에 반영함

### 이 시스템을 활용하는 방법 (개발자용)

| 클래스 | 받아쓰는 방법 | 제공 기능 |
|---|---|---|
| `IAudioRaiser` | `[Preserve, Inject] private IAudioRaiser _audioChannel;` | `RaisePlay(AudioPlayRequest)` 소리 재생, `RaiseStop(SoundType)` 소리 정지 |
| `AudioPlayRequest` | 정적 함수로 생성 | `Create(id)` 2D 재생, `CreateAt(id, 위치)` 3D 재생 |
| `SoundType` | enum 직접 사용 | 소리 이름표. 새 소리는 여기에 값을 추가함 |
| `ISoundSettings` | `[Preserve, Inject] private ISoundSettings _soundSettings;` | `GetVolume(category)` 볼륨 읽기, `SetVolume(category, 값)` 볼륨 쓰기, `OnChanged` 변경 알림 |
| `VolumeCategory` | enum 직접 사용 | 볼륨 묶음 이름표 (Master, Bgm, Sfx, Enemy, Player, Environment, Ui) |

### 이 시스템을 활용하는 방법 (비개발자용)

| 클래스 | 만드는 법 | 배치 위치 | 채울 값 |
|---|---|---|---|
| `AudioChannel` | 프로젝트 창 우클릭 → Create → Refactoring/Audio/AudioChannel | 씬의 `DataContainer` 그룹에 등록 | 없음 |
| `AudioCatalog` | 프로젝트 창 우클릭 → Create → Refactoring/Audio/AudioCatalog | 씬의 `DataContainer` 그룹에 등록 | `Entries` 목록에 소리 항목 추가 |
| `AudioCatalogEntry` | `AudioCatalog`의 `Entries`에서 + 버튼 | `AudioCatalog` 안 | Id(SoundType 선택), Clips(여러 개면 무작위), Volume(0~1), Pitch, SpatialBlend(0=2D, 1=3D), MinDistance, MaxDistance, Loop, Output(믹서 그룹) |
| `AudioPlayer` | 빈 오브젝트에 컴포넌트 추가 | 씬에 하나 | 없음 |
| `AudioPlayerData` | 프로젝트 창 우클릭 → Create → Refactoring/Audio/AudioPlayerData | 씬의 `DataContainer` 그룹에 등록 | `InitialVoices`(처음 만들어 둘 재생용 AudioSource 개수, 기본 8) |
| `VolumeController` | 빈 오브젝트에 컴포넌트 추가 | 씬에 하나 | `Mixer`에 오디오 믹서 연결. 믹서에서 `VolumeCategory` 이름과 같은 파라미터(MasterVolume 등 7개)를 노출해야 함 |

---

## CameraSystem
- 확정: 2026-09-14
- 캐릭터 추적 카메라 전환·락온·시점 조작과 셰이크·줌·투과 연출을 맡는 시스템

### 이 시스템을 활용하는 방법 (개발자용)

| 클래스/인터페이스 | 받아쓰는 방법 | 제공 기능 |
|---|---|---|
| `ILockOnState` | `[Inject] private ILockOnState _lockOn;` | `IsLockOn`(bool) 조회, `OnLockOnChanged` 구독 |
| `ILockOnTarget` | `[Inject] private ILockOnTarget _lockOnTarget;` | `LockedTarget`(Collider) 조회 |
| `ILockOnTargetDetector` | `[Inject] private ILockOnTargetDetector _detector;` | `Candidates`(IReadOnlyList\<Collider\>) 조회 |
| `LockOnController` | 인터페이스 2개(위)로만 받을 것. 구체 클래스 직접 참조는 비권장(현재 UISystem의 LockOnMarker가 이렇게 쓰고 있음 — `Docs/Retrospective.md` 3-3 기록된 문제) | 없음 |
| `IMouseSettings` | `[Inject(true)] private IMouseSettings _mouseSettings;` | `SpeedX`/`SpeedY` get·set, `OnChanged` 구독 |
| `IPointerLookControl` | `[Inject(true)] private IPointerLookControl _pointerLook;` | `SetPointerLookEnabled(bool)` |
| `CameraShakeDataEntry` | PlayerSystem `StateData`에 `CameraShakeDataEntry[]` 필드로 선언 | 상태 진입 시 `IStartData`·`IPlayerCameraShake`로 셰이크 값 전달 |
| `CameraZoomDataEntry` | PlayerSystem `StateData`에 `CameraZoomDataEntry[]` 필드로 선언 | 상태 진입 시 `IPlayerCameraZoom`으로 줌 값 전달 |

### 이 시스템을 활용하는 방법 (비개발자용)

| 클래스 | 만드는 법 | 배치 위치 | 채울 수치 |
|---|---|---|---|
| `CameraRole` | 컴포넌트 추가(CinemachineCamera 필수) | 각 카메라 오브젝트 | `Kind`(Default/LockOn) |
| `CameraSwitcher` | 컴포넌트 추가 | 카메라 매니저 오브젝트 | 없음(씬의 CameraRole 자동 수집) |
| `LockOnController` | 컴포넌트 추가 | 락온 담당 오브젝트 | `ReleaseDistance` |
| `LockOnTargetDetector` | 컴포넌트 추가 | 락온 담당 오브젝트 | `DetectRange`, `TargetMask`, `ObstacleMask`, `IsDebugDraw` |
| `MouseSpeedApplier` | 컴포넌트 추가(CinemachineInputAxisController 필수) | 플레이어 카메라 프리팹 | 없음 |
| `PlayerCameraLockHandler` | 컴포넌트 추가(CinemachineInputAxisController 필수) | 플레이어 카메라 프리팹 | 없음 |
| `SeeThroughWall` | 컴포넌트 추가 | 카메라 오브젝트 | `HoleSize`, `Opacity`, `EdgeSoftness`, `OccluderMask` |
| `CameraShakeDataEntry` | PlayerSystem 상태 데이터 에셋의 CameraShake 목록에 항목 추가 | 상태 정의 에셋(PlayerSystem) | `Name`, `StartProgress`, `Shake`(ImpulseShape·AmplitudeGain·FrequencyGain·Duration·Velocity) |
| `PlayerCameraShake` | 컴포넌트 추가(CinemachineImpulseSource 필수) | 플레이어 오브젝트 | `MaxSameStateStack`, `AmplitudeGainPerStack` |
| `PlayerCameraShakeData` | 메뉴 `Data/PlayerCameraShakeData`로 SO 생성. DataContainer 등록 필요 | 데이터 폴더 | `ShakeList`(State별 ShakeData) |
| `PlayerCameraShakeHandler` | 컴포넌트 추가(CinemachineImpulseSource 필수) | 스킬 연출용 카메라 오브젝트 | 없음 |
| `CameraZoomDataEntry` | PlayerSystem 상태 데이터 에셋의 CameraZoom 목록에 항목 추가 | 상태 정의 에셋(PlayerSystem) | `Name`, `StartProgress`, `DistanceScale`, `ZoomOutTime`, `ZoomHoldTime`, `ZoomInTime` |
| `PlayerCameraZoomHandler` | 컴포넌트 추가 | 스킬 연출용 카메라 오브젝트 | 없음 |

---

## CombatSystem
- 확정: 2026-09-15
- 피해 적용 계약(IDamageable)과 피해 데이터(DamageInfo, InkColorType)를 시스템 간에 공유함

### 이 시스템을 활용하는 방법 (개발자용)

| 클래스 | 받아쓰는 방법 | 제공 기능 |
|---|---|---|
| `IDamageable` | 피해 받을 대상 컴포넌트에 구현 | `ApplyDamage(DamageInfo)`로 피해 통보 받음 |
| `DamageInfo` | `ApplyDamage` 호출 시 값 채워 넘김 | `Damager`/`Amount`/`HitPoint`/`Color`/`InkStack` 필드 전달 |
| `InkColorType` | `DamageInfo.Color`에 값 넣음 | 없음 |

### 이 시스템을 활용하는 방법 (비개발자용)

에디터 작업이 필요한 클래스 없음.

---

## GameStateSystem

---

## InputSystem

---

## LanguageSystem
- 확정: 2026-09-15
- 게임 UI 문구를 언어별로 번역·표시하고, 언어 전환 기능을 제공하는 시스템

### 이 시스템을 활용하는 방법 (개발자용)

| 클래스/인터페이스 | 받아쓰는 방법 | 제공 기능 |
|---|---|---|
| `ILanguageSettings` | `[Preserve, Inject(true)] private ILanguageSettings _language;` | `Current` get·set, `GetText(key)`, `GetFont()`, `OnChanged` 구독 |
| `ITextTableData` | `[Preserve, Inject] private ITextTableData _table;` | `GetText(key, language)`, `GetFont(language)` |
| `LanguageType` | enum 직접 사용 | 지원 언어 이름표(Korean, English) |

### 이 시스템을 활용하는 방법 (비개발자용)

| 클래스 | 만드는 법 | 배치 위치 | 채울 값 |
|---|---|---|---|
| `LanguageButton` | 컴포넌트 추가(Button 필수) | 언어 선택 버튼 오브젝트 | `_target`(이 버튼이 맡을 언어) |
| `LocalizedText` | 컴포넌트 추가(TextMeshProUGUI 필수) | 번역이 필요한 글자 오브젝트 | `_key`(표의 번역 키, 드롭다운 선택) |
| `TextTableData` | 프로젝트 창 우클릭 → Create → Refactoring/TextTableData | `Assets/5.Data/Language/` | `_entries`(키별 한국어·영어 문장), `_koreanFont`, `_englishFont`. `DataContainer` 등록 필요 |

---

## PlatformSystem
- 확정: 2026-09-15
- 빌드된 플랫폼(모바일/PC)에 맞춰 오브젝트 켜짐, FPS, 해상도, 카메라 포인터 조작을 자동 전환한다.

### 이 시스템을 활용하는 방법 (개발자용)

| 클래스 | 받아쓰는 방법 | 제공 기능 |
|---|---|---|
| `PlatformObject` | `[Inject(true)] List<PlatformObject>`로 받음 | 없음 (플래그 데이터 전달용) |

### 이 시스템을 활용하는 방법 (비개발자용)

| 클래스 | 만드는 법 | 배치 위치 | 채울 값 |
|---|---|---|---|
| `PlatformObject` | 컴포넌트 추가 | 플랫폼별로 켜고 끌 오브젝트 | `Is Mobile Only` 체크(모바일 전용)/해제(PC 전용) |
| `PlatformController` | 컴포넌트 추가 | 씬에 하나 | `Mobile Target FPS`, `Window Target FPS`, `Mobile Pixel Budget` |

---

## PlayerSystem
- 확정: 2026-09-16
- 플레이어 상태 전환·상태별 이벤트 발행·현재 상태 공유를 담당함

### 이 시스템을 활용하는 방법 (개발자용)

| 클래스 | 받아쓰는 방법 | 제공 기능 |
|---|---|---|
| `IStateTriggerRaiser`(`PlayerStateTriggerChannel`이 구현) | `[Preserve, Inject] private IStateTriggerRaiser _raiser;` | `RaiseTrigger(StateTriggerType)`로 상태 전환 요청 |
| `IStateTriggerSubscriber`(`PlayerStateTriggerChannel`이 구현) | `[Preserve, Inject] private IStateTriggerSubscriber _sub;` | `SubscribeTrigger`/`UnsubscribeTrigger`로 전환 트리거 구독 |
| `IPlayerStateEventRaiser`(`PlayerStateEventChannel`이 구현) | `[Preserve, Inject] private IPlayerStateEventRaiser _raiser;` | `Raise`/`RaiseEnd`/`RaiseReset`로 상태 구간 이벤트 발행 |
| `IPlayerStateEventSubscriber`(`PlayerStateEventChannel`이 구현) | `[Preserve, Inject] private IPlayerStateEventSubscriber _sub;` | `Register(StateEventCategory, open, close)`로 구간 시작·종료 구독, `IDisposable` 반환 |
| `ICurrentStateProvider`(`PlayerCurrentStateChannel`이 구현) | `[Preserve, Inject(true)] private ICurrentStateProvider _provider;` | `CurrentState` 조회, `StateChanged` 이벤트 |
| `ICurrentStateWriter`(`PlayerCurrentStateChannel`이 구현) | `[Preserve, Inject(true)] private ICurrentStateWriter _writer;` | `SetCurrentState(PlayerStateType)`(PlayerStateMachine 전용) |
| `StateEventCategory`(enum) | `Register`/`Raise` 호출 시 카테고리 지정 | 없음 |
| `IStartData` | 구간 데이터 클래스가 구현, `Register`의 open 콜백 매개변수로 받음 | `StartProgress` 조회 |
| `IMotionControl` | 구간 데이터 클래스가 선택적으로 구현 | `Duration`, `UntilEnd` 조회 |
| `StateTriggerType`(enum) | `RaiseTrigger`/`SubscribeTrigger` 인자 | 없음 |
| `PlayerStateType`(enum) | `ICurrentStateProvider.CurrentState` 값 | 없음 |
| `CloseEventType`(enum) | `Register`의 close 콜백 인자(End/Reset 구분) | 없음 |

### 이 시스템을 활용하는 방법 (비개발자용)

| 클래스 | 만드는 법 | 배치 위치 | 채울 값 |
|---|---|---|---|
| `PlayerStateTriggerChannel` | Assets 우클릭 > Create > EventChannel/PlayerStateTriggerChannel | `5.Data/Player/Event` | 없음 |
| `PlayerStateEventChannel` | Assets 우클릭 > Create > EventChannel/PlayerStateEventChannel | `5.Data/Player/Event` | 없음 |
| `PlayerCurrentStateChannel` | Assets 우클릭 > Create > EventChannel/PlayerCurrentStateChannel | `5.Data/Player/Event` | 없음 |
| `StateData` | Assets 우클릭 > Create > Data/StateData | `5.Data/Player/State` 이하(캐릭터별 폴더) | `_stateType`, `_isLooping`, `_cooldown`, 구간 배열(`_inputBlock` 등), 이벤트 배열(`_effect` 등) |
| `PlayerStateMachine` | 캐릭터 프리팹에 컴포넌트 추가 | `ICharacterComponentSource` 있는 오브젝트 | `_stateDataList`에 이 캐릭터가 쓸 `StateData` 전부 등록, 3개 채널 DI로 주입 |
| `PlayerStateInputGate` | 캐릭터 프리팹에 컴포넌트 추가 | `PlayerStateMachine`과 같은 오브젝트 | 없음(전부 DI 주입) |

`StateData`는 `IDataProvider`로 DI에 등록되므로 DataContainer 등록도 필요함.

---

## SaveSystem
- 확정: 2026-09-15
- 게임 진행 상황과 사용자 설정(입력·사운드)을 파일로 저장하고 불러옴

### 이 시스템을 활용하는 방법 (개발자용)

| 클래스 | 받아쓰는 방법 | 제공 기능 |
|---|---|---|
| `ISaveService`(`SaveManager`가 구현) | `[Preserve, Inject(true)] private ISaveService _saveService;` | `Save`/`Load<T>()`(슬롯 없는 버전), `Save`/`Load`/`Delete`/`Exists<T>(slot)`(슬롯 버전) |
| `ISaveSlots`(`SlotLoadRunner`가 구현) | `[Preserve, Inject(true)] private ISaveSlots _saveSlots;` | `GetSlots()` 슬롯 목록, `LoadSlot(index)` 그 칸으로 게임 시작 |
| `SaveSlotManager` | `[Preserve, Inject(true)] private SaveSlotManager _slots;` — 구체 클래스 직접 참조라 GimmickSystem 3곳에 인터페이스화 TODO 남아있음 | `GetSlots`, `Save`, `TakeSlot`, `GetCurrentData`, `OverwriteCurrent`, `DeleteAll` |
| `GameStateSaver` | `FindFirstObjectByType<GameStateSaver>()`으로 찾음(SlotLoadRunner 내부용) | `Restore(GameSaveData)` 저장값대로 캐릭터·자리·체력·오브젝트 상태 복원 |
| `ISaveData`(`GameSaveData`/`InputData`/`SoundData`가 구현) | `ISaveService`의 `T`로 사용 | 없음(데이터만 담음) |

### 이 시스템을 활용하는 방법 (비개발자용)

| 클래스 | 만드는 법 | 배치 위치 | 채울 값 |
|---|---|---|---|
| `SaveManager` | 빈 오브젝트에 컴포넌트 추가 | 씬에 하나 | 없음 |
| `SaveSlotManager` | 빈 오브젝트에 컴포넌트 추가 | 씬에 하나 | `_slotCount`(세이브 슬롯 개수, 기본 4) |
| `SlotLoadRunner` | 빈 오브젝트에 컴포넌트 추가 | 씬에 하나 | 없음 |
| `GameStateSaver` | 빈 오브젝트에 컴포넌트 추가 | 씬마다 하나 | 없음 |

SO 에셋·DataContainer 등록은 없음.

---

## SettingsSystem


---

## UISystem
