# System Usage Guide (시스템 활용 가이드)

> **시스템 경계 = `Assets/1.Code/Scripts/` 바로 아래 폴더 하나.**
> **시스템 내부 기능 경계 = 시스템 폴더 바로 아래 폴더 하나**
> `Old/`, `_Test`, `PerformanceTest`, `Editor`는 대상에서 제외한다.

---

## 항목 형식

```
## <시스템명>
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

---

## AudioSystem
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
| `VolumeController` | 빈 오브젝트에 컴포넌트 추가 | 씬에 하나 | `Mixer`에 오디오 믹서 연결. 믹서에서 `VolumeCategory` 이름과 같은 파라미터(MasterVolume 등 7개)를 노출해야 함 |

---

## CameraSystem


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
