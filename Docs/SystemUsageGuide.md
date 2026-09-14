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
| `AudioChannel` | `[Preserve, Inject] private AudioChannel _audioChannel;` | `RaisePlay(AudioPlayRequest)` 소리 재생, `RaiseStop(SoundType)` 소리 정지 |
| `AudioPlayRequest` | 정적 함수로 생성 | `Create(id)` 2D 재생, `CreateAt(id, 위치)` 3D 재생, `CreateFollowing(id, Transform)` 대상 추종 재생 |
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


---

## GameStateSystem

---

## InputSystem

---

## LanguageSystem


---

## PlatformSystem


---

## PlayerSystem


---

## SaveSystem


---

## SettingsSystem


---

## UISystem
