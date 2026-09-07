# System Usage Guide (시스템 활용 가이드)

> **시스템 경계 = `Assets/1.Code/Scripts/` 바로 아래 폴더 하나.**
> 하위 폴더(`PlayerSystem/Movement` 등)는 그 시스템의 모듈이지 독립 시스템이 아니다.
> `Old/`, `_Test`, `PerformanceTest`, `Editor`는 대상에서 제외한다.

## 이 파일을 쓰는 법

- **코드를 쓰기 전**: 해당 시스템 항목을 읽고 그 규칙대로 작성한다.
- **평가할 때**: 여기 적힌 규칙을 어겼으면 위반이다.
- **여기 없는 시스템**을 코드에서 발견하면 `system-evaluation`을 돌려 항목을 채운다.
- **상태**가 `미확정`인 항목이 하나라도 있으면 클래스 평가를 진행하지 않는다.
  (미확정 규칙이 판정을 오염시키므로 확정부터 받는다.)

## 수치를 어디 둘 것인가 (전역 규칙)

튜닝 대상이 되는 값(수치·플래그·키·클립 참조 등)은 둘 중 하나에 둔다.

- **SO로 뺀다** — 게임 규칙·밸런스 값(대시 거리, 무적 시간, 쿨타임).
  바꿀 때 모든 개체가 같이 바뀌어야 하는 값이다. `DataContainer`의 그룹에 넣는다.
- **`[SerializeField]`로 둔다** — 그 개체의 배치·개성(순찰 반경, 문 열림 각도, 추종 오프셋).
  `// 개체별` 주석을 붙여 판단이 끝난 값임을 표시한다.

판별 기준 한 줄: **"이 값을 바꿀 때 다른 개체도 같이 바뀌길 원하나?"** — 예면 SO.

- 판단이 서지 않으면 사용자에게 묻는다. `// 개체별` 주석이 있으면 묻지 않는다.
- 기존 클래스의 매직넘버도 같은 기준으로 본다.
- 예외: 직접 만들지 않은 시스템(기믹·몬스터)은 대상에서 제외한다.

---

## 시스템끼리 어떻게 연결할 것인가 (전역 규칙)

- **시키는 일은 인터페이스로만 한다.** 구현 클래스(`AudioPlayer`, `SaveManager` 등)를 직접 참조하지 않는다.
- **주고받는 데이터 타입(enum·struct·SO)은 주인 시스템 하나가 정의한다.** 나머지 시스템은 그대로 받아 넘기고, 같은 뜻의 타입을 자기 폴더에 다시 만들지 않는다.
- **참조해도 되는 외부 시스템은 이 문서의 `외부 시스템 연결`에 적힌 것뿐이다.** 거기 없는 시스템을 끌어다 쓰면 위반이다.

경계에서 남의 타입을 자기 타입으로 바꿔 받는 변환 계층은 두지 않는다.
그 타입을 우리가 직접 고칠 수 있고, 쓰는 곳이 한두 곳이면 변환 코드만 늘어난다.
쓰는 시스템이 서넛으로 늘어난 뒤에 그때 판단한다.

---

## 항목 형식

이 문서를 읽는 목적은 하나다. **이 시스템이 안팎으로 어떻게 연결되는가.**
그래서 항목은 아래 세 덩어리만 쓴다.

```
## <시스템명>

- 상태: 확정 | 미확정
- 역할: 한 줄

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 진입점: 외부가 이 시스템을 쓸 때 건드리는 타입
- [<쓰는 쪽> → <쓰이는 쪽>] <어떻게 받는지>. <무엇을 기준으로 무엇을 하는지>.

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- <쓰는 외부 시스템> — <무엇을 위해, 어떤 타입으로 어떻게 받는지> (없으면 `없음`)

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 안 건드리고 씬·프리팹·SO에서 할 일.

- <씬·프리팹·SO에서 무엇을 붙이고 무엇을 채우는지 한 줄>
```

- **시스템 안쪽**: 이 시스템 폴더 안의 코드끼리 이어지는 경로.
- **다른 시스템과의 연결**: 다른 시스템을 쓰는 지점 전부. 여기 없는 시스템을 끌어다 쓰면 위반이다.
- **에디터에서 쓰는 법**: 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.
  인스펙터에 보이는 컴포넌트·에셋 이름은 실물이므로 그대로 적는다. 주입 문법 같은 코드 작성법만 뺀다.

가독성: `##`·`###` 뒤와 절 사이에 빈 줄을 둔다. 한 항목은 한 줄이다. 시스템끼리는 `---`로 나눈다.

---

## DISystem

- 상태: 확정
- 역할: 시스템 간 참조를 어트리뷰트 주입으로 연결한다.

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 진입점: `[Inject]` 어트리뷰트, `IDataProvider`
- [AttributeInjector → 씬 오브젝트] `FindObjectsByType`으로 직접 수집함. `Awake()`에서 모은 뒤 `[Inject]` 필드를 채운다. 비활성 오브젝트도 포함한다.
- [AttributeInjector → DataContainer] `IDataProvider` 인터페이스로 받음. SO는 씬에서 못 찾으므로 `ProvideData()`가 넘겨준 것만 등록한다.
- 등록 키는 구체 타입·부모 타입·인터페이스 전부다. 한 곳에서만 쓰는 참조는 구체 타입으로 받아도 된다.
- 한계: 주입은 씬 `Awake` 1회뿐이라 런타임에 스폰한 프리팹은 못 받는다. 스포너가 미리 받아 넘겨 우회 중이다(Enemy.cs:156, WisuSpawnPhase.cs:12).
- 한계: 같은 타입 구현이 2개 이상이면 첫 번째만 주입되고 LogWarning만 남는다(AttributeInjector.cs:196).

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.
>
> **이 항목만 예외다.** DISystem이 내주는 것은 데이터가 아니라 *참조를 받는 방법* 자체라서,
> 다른 시스템이 지켜야 할 규칙을 여기에 적는다. 다른 시스템 항목은 이렇게 쓰지 않는다.

- 받아오는 것: 없음. 모든 시스템이 이 시스템을 쓰지만, 이 시스템은 남을 참조하지 않는다.
- 다른 시스템 참조는 `[Inject]`로 받는다. 없으면 기능이 안 되는 의존은 `[Inject]`, 없어도 도는 의존은 `[Inject(true)]`.
- `[SerializeField]`는 자기 GameObject 컴포넌트나 인스펙터 조정값에만 쓴다.
- 부모 클래스에 `[Inject]` 필드를 둘 때는 `protected`로 둔다. `private`는 자식 스캔에서 안 잡혀 경고 없이 null로 남는다.
- 씬을 넘겨 값을 유지할 때 `DontDestroyOnLoad`로 오브젝트를 살리지 않는다. 씬 사본과 함께 2개가 등록된다. `static` 값만 남긴다.

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 안 건드리고 씬·프리팹·SO에서 할 일.

- 씬마다 `AttributeInjector`가 놓여 있어야 한다. 빠지면 그 씬의 연결이 전부 끊긴다.
- 코드에서 쓰는 SO 에셋은 씬의 `DataContainer` 그룹에 등록해야 한다. 빠뜨리면 콘솔에 경고가 뜬다.

---

## AudioSystem

- 상태: 확정
- 역할: 재생 요청을 받아 AudioSource 풀로 소리를 내고, 믹서 볼륨을 설정값에 맞춘다.

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 진입점: `AudioChannel`(재생 요청), `ISoundSettings`(볼륨 값)
- [AudioPlayer → AudioChannel] SO를 `[Inject]`로 받음. 채널의 재생 요청 이벤트를 구독해 AudioSource 풀에서 하나 꺼내 재생한다.
- [AudioPlayer → AudioCatalog] SO를 `[Inject]`로 받음. 요청에 실린 `SoundType`을 열쇠로 클립과 소리 수치를 찾는다.
- [SoundSettings → 저장 파일] `SettingsHolder` 상속으로 받음. `VolumeCategory` 이름을 열쇠로 저장해 enum 순서가 바뀌어도 값이 안 밀린다.
- [VolumeController → ISoundSettings] 인터페이스를 `[Inject]`로 받음. 변경 알림을 듣고 그 값을 AudioMixer 파라미터에 반영한다.
- 외부는 `AudioChannel`·`ISoundSettings`만 본다. `AudioPlayer`·`AudioCatalog`·`VolumeController`·`AudioSource`는 직접 참조하지 않는다.
- 소리 수치(볼륨/피치/거리/루프/믹서 그룹)는 코드가 아니라 `AudioCatalog` SO에서 정한다.
- 같은 클립이라도 쓰임이 다르면 `SoundType`을 따로 만든다.
- 예외 현황: BGM과 적 소리는 이 시스템을 거치지 않고 씬의 AudioSource를 직접 쓴다.

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- SettingsSystem — 설정 저장 틀을 쓰기 위해 `SoundSettings`가 `SettingsHolder<SoundSettingsData>`를 상속한다. 저장 시점은 `SettingsSaver`가 정한다.
- DISystem — `AudioChannel`·`AudioCatalog` SO를 `DataContainer`에 등록하고 `[Inject]`로 받는다.
- 내주는 것: `AudioChannel`(재생 요청), `ISoundSettings`와 `VolumeCategory`(볼륨 읽기·쓰기). `VolumeCategory`의 주인은 이 시스템이다.
- 호출 예: `_audioChannel.RaisePlay(AudioPlayRequest.Create(SoundType.UIClick));` / `CreateAt(id, point)` 3D / `CreateFollowing(id, tr)` 추종 / `RaiseStop(id)`

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 안 건드리고 씬·프리팹·SO에서 할 일.

- 새 소리를 추가하려면 `AudioCatalog` 에셋에 줄을 하나 만들고 클립과 볼륨·거리 값을 채운다.
- 소리가 안 나면 먼저 그 소리의 `SoundType`이 `AudioCatalog`에 있는지 본다.
- 볼륨 슬라이더는 어떤 소리 묶음을 만질지 인스펙터에서 고른다.
- 그 묶음 이름은 오디오 믹서에 노출한 파라미터 이름과 반드시 같아야 한다. 다르면 슬라이더를 움직여도 소리가 안 변한다.

---

## CameraSystem

- 상태: 미확정
- 역할: (미확정)

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- (미확정)

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- (미확정)

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- (미확정)

---

## CombatSystem

- 상태: 미확정
- 역할: (미확정)

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- (미확정)

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- (미확정)

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- (미확정)

---

## EnemySystem

- 상태: 미확정
- 역할: (미확정)

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- (미확정)

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- (미확정)

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- (미확정)

---

## GameStateSystem

- 상태: 확정
- 역할: 게임 모드 스택을 보유하고 현재 모드를 알린다. 모드에 딸린 전역 효과(정지·커서)를 적용한다.

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 진입점: `IGameStateProvider`(읽기), `IGameStateController`(쓰기)
- [GameStateManager → 모드 스택] 자기 필드로 직접 보유함. 현재 모드의 유일한 주인이고, 바뀌면 `OnChanged`로 알린다.
- [GamePauseController·CursorController → IGameStateProvider] 인터페이스를 `[Inject]`로 받음. 현재 모드를 기준으로 각자 `Time.timeScale`과 `Cursor`를 바꾼다.
- GamePlay는 스택 바닥 고정이다. Push·Pop 대상이 아니다.
- Push는 현재 top보다 우선순위가 높을 때만 얹힌다. 우선순위는 `Priority`에서만 정한다.
- Pop은 자기가 얹은 모드를 인자로 넘긴다. 남의 모드는 내려가지 않는다.
- 전역 상태를 바꾸는 주체는 효과마다 하나뿐이다. 새 효과가 필요하면 컨트롤러를 하나 더 만들고 기존 것은 건드리지 않는다.

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- 없음. 이 시스템은 다른 시스템을 참조하지 않는다.
- 내주는 것: `IGameStateProvider`·`IGameStateController`와 모드 이름표 `GameStateType`. 그 주인은 이 시스템이다.
- 읽기만 하면 `IGameStateProvider`, 바꾸려면 `IGameStateController`를 받는다. 둘을 함께 받지 않는다.
- 호출 예: `_gameState.OnChanged += HandleStateChanged;` / `_gameState.Push(GameStateType.Cutscene);` / `Pop(GameStateType.Cutscene)`

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 안 건드리고 씬·프리팹·SO에서 할 일.

- 씬마다 게임 상태 오브젝트가 놓여 있어야 한다. 빠지면 일시정지와 커서가 동작하지 않는다.
- 메뉴를 열었는데 게임이 안 멈추거나 커서가 안 보이면 이 오브젝트부터 확인한다.

---

## GimmickSystem

- 상태: 미확정
- 역할: (미확정)

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- (미확정)

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- (미확정)

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- (미확정)

---

## InputSystem

- 상태: 확정
- 역할: 사용자 입력을 수집해 현재 게임 모드의 처리기에게만 전달한다. 해석은 하지 않는다.

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 진입점: `IInputPressedProvider`, `IInputMoveProvider`, `IMenuInputProvider`, `ICutsceneInputProvider`
- [InputHub → IDomainInputHandler] `List<>`로 전부 `[Inject]` 받음. 각 처리기의 `Context` 값을 기준으로 현재 게임 모드에 맞는 하나에게만 입력을 넘긴다.
- [InputHub → IInputKeySettings] 인터페이스를 `[Inject(true)]`로 받음. 저장된 리바인딩 문자열을 시작할 때 액션 에셋에 덮어씌운다.
- 처리기는 발행만 한다. 입력의 의미 해석·차단·버퍼는 구독자가 맡는다.
- 새 게임 모드는 `IDomainInputHandler` 구현 추가로만 늘린다. 한 모드에 처리기는 하나다.
- 컷씬 중에는 Interaction·Menu만 발행한다. 스킵이냐 메뉴 열기냐는 구독자가 판단한다.
- 새 액션은 `InputActionType` enum과 `.inputactions` 에셋에 같은 이름으로 하나씩 추가한다.

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- GameStateSystem — 현재 모드를 알기 위해 `IGameStateProvider`를 `[Inject]`로 받는다. 없으면 즉시 예외.
- SettingsSystem — 저장 틀을 쓰기 위해 `InputKeySettings`가 `SettingsHolder<InputKeySettingsData>`를 상속한다.
- DISystem — `InputActionAsset`을 `DataContainer`에 등록하고 `[Inject(true)]`로 받는다.
- 내주는 것: 위 네 개의 Provider와 `IInputKeySettings`(리바인딩), 액션 이름표 `InputActionType`.
- 호출 예: `[Preserve, Inject(true)] private IInputPressedProvider _input;` 후 `_input.OnInputPressed += HandlePressed;`

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 안 건드리고 씬·프리팹·SO에서 할 일.

- 조작키를 바꾸려면 `.inputactions` 에셋에서 해당 액션의 키를 다시 지정한다.
- 모바일 화면 버튼은 `OnScreenButton`과 `MobileInputButton`을 같이 붙인다. 그러면 키보드와 똑같이 취급된다.
- 그 버튼의 `controlPath`는 해당 액션의 현재 키와 맞춰야 한다. 게임 중 리바인딩하면 자동으로 따라간다.
- 이동은 키 변경 대상이 아니다. 모바일 이동 조이스틱은 `OnScreenStick`을 쓴다.

---

## LanguageSystem

- 상태: 확정
- 역할: 지금 언어를 보유하고, 키에 맞는 번역 문장과 언어별 폰트를 준다.

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 진입점: `ILanguageSettings`
- [LanguageSettings → TextTableData] SO를 `[Inject]`로 받음. 지금 언어를 기준으로 `GetText`·`GetFont`를 대신 호출해 결과만 넘긴다.
- [LanguageButton → ILanguageSettings] 인터페이스를 `[Inject(true)]`로 받음. 버튼에 지정된 `LanguageType`을 `Current` 세터에 넣어 언어를 바꾼다.
- [LocalizedText → ILanguageSettings] 인터페이스를 `[Inject(true)]`로 받음. `OnChanged` 알림을 받아 자기 키에 맞는 글자와 폰트만 갱신한다.

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- SettingsSystem — 값 보관·저장을 위해 `SettingsHolder<LanguageSettingsData>`를 상속한다. 저장 시점은 `SettingsSaver`가 정한다.
- DISystem — `TextTableData` SO를 `DataContainer`에 등록하고 `[Inject]`로 받는다.
- 내주는 것: `ILanguageSettings`(외부는 `[Inject(true)]`로 받는다)와 언어 이름표 `LanguageType`.

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 안 건드리고 씬·프리팹·SO에서 할 일.

- 글자를 번역 대상으로 만들려면 그 TMP 오브젝트에 `LocalizedText`를 붙이고 키를 드롭다운에서 고른다.
- 번역 문장을 고치거나 추가하려면 `TextTableData` 에셋의 표에서 한국어·영어 칸을 채운다.
- 언어별 글꼴을 바꾸려면 같은 에셋의 한국어 폰트·영어 폰트 칸을 갈아 끼운다.
- 언어 전환 버튼은 Button 오브젝트에 `LanguageButton`을 붙이고 맡을 언어를 고른다.
- 새 씬을 만들면 씬의 `DataContainer`에 `TextTableData` 에셋을 등록해야 글자가 나온다.

---

## PlatformSystem

- 상태: 확정
- 역할: 빌드된 플랫폼(PC/모바일)에 맞춰 오브젝트와 조작을 한 번에 맞춘다.

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 진입점: `PlatformObject`(대상 표식). 코드에서 부르는 진입점은 없다.
- [PlatformController → PlatformObject] `List<>`로 전부 `[Inject(true)]` 받음. `IsMobileOnly` 값과 현재 플랫폼을 비교해 `Start()`에서 오브젝트를 켜고 끈다.
- [PlatformController → 빌드 플랫폼] `Application.isMobilePlatform`을 직접 읽음. 이 값 하나가 유일한 판단 기준이고, 유저가 고르는 설정값은 두지 않는다.
- 참조가 빠지면 조용히 넘기지 않고 무엇이 안 되는지 경고를 남긴다.

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- CameraSystem — 모바일에서 포인터로 카메라가 돌지 않게 `IPointerLookControl`을 `[Inject(true)]`로 받아 호출한다.
- DISystem — `InputActionAsset`을 `DataContainer`에서 `[Inject(true)]`로 받는다. 이 에셋은 InputSystem 소유가 아니라 유니티 패키지 SO라 InputSystem을 거치지 않는다.
- 내주는 것: 없음. 다른 시스템이 이 시스템을 코드로 부르지 않는다.

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- 특정 플랫폼에서만 보여야 하는 오브젝트에는 `PlatformObject`를 붙이고, 모바일 전용이면 체크한다.
- 씬마다 `PlatformController`가 놓여 있어야 한다. 빠지면 PC·모바일 오브젝트가 전부 같이 켜진다.
- 플랫폼별 목표 프레임은 `PlatformController` 인스펙터에서 각각 정한다.
- 안 켜지거나 안 꺼지면 콘솔 경고를 먼저 본다. 무엇이 빠졌는지 이름이 찍힌다.

---

## PlayerSystem

- 상태: 미확정
- 역할: (미확정)

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 하위 폴더(`Movement`, `Health`, `Hitbox`, `States` 등)는 Player 전용 모듈이다. Player 밖에서도 쓰이게 되면 그때 독립 시스템으로 승격한다.

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- (미확정)

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- (미확정)

---

## SaveSystem

- 상태: 미확정
- 역할: (미확정)

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- (미확정)

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- (미확정)

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- (미확정)

---

## SettingsSystem

- 상태: 확정
- 역할: 설정값을 파일에 저장하고 되불러오는 틀을 준다. 무슨 설정인지는 모른다.

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- 진입점: `SettingsHolder<TData>`(설정 추가), `ISettingsHolder`(일괄 저장)
- [SettingsHolder → ISaveService] 인터페이스를 `[Inject]`로 받음. `Data`를 처음 읽는 순간 파일에서 한 번만 불러오고, 이후로는 메모리 값을 준다.
- [SettingsSaver → ISettingsHolder] `List<>`로 전부 `[Inject]` 받음. 설정창이 꺼지는 `OnDisable`에서 holder 전부에 `Save()`를 시킨다.
- 설정 holder 자체는 이 폴더에 두지 않는다. 값을 쓰는 시스템 폴더에 둔다(`SoundSettings`는 AudioSystem).

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- SaveSystem — 파일 읽기·쓰기를 위해 `ISaveService`를 `[Inject]`로 받고, 저장 규격 `ISaveData`를 `TData` 제약으로 쓴다.
- DISystem — `SettingsSaver`가 `List<ISettingsHolder>`로 모든 holder를 주입받는다.
- 내주는 것: `SettingsHolder<TData>`(상속용)와 `ISettingsHolder`(일괄 저장용).

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- 설정값은 설정창을 닫는 순간 저장된다. 창을 안 닫으면 저장되지 않는다.
- 설정 오브젝트는 씬마다 배치돼 있어야 한다. 빠진 씬에서는 설정을 못 읽는다.

---

## UISystem

- 상태: 미확정
- 역할: (미확정)

### 시스템 안쪽 (개발자용)

> 이 폴더 안의 클래스끼리 어떻게 이어지는지. 밖에서는 몰라도 되는 내용이다.

- (미확정)

### 다른 시스템과의 연결 (개발자용)

> 이 시스템이 밖에서 무엇을 받아오고, 밖에 무엇을 내주는지.

- (미확정)

### 에디터에서 쓰는 법 (비개발자용)

> 코드를 고치는 것 말고 에디터에서 할 수 있는 일 전부.

- (미확정)
