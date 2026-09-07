# 한계·주의사항·다음 프로젝트 과제

이 문서는 코드 자체가 아니라 **구조가 못 하는 것**과 **그 이유**를 적는다.
사실과 근거만 적고, 판단이 갈리는 부분은 그렇다고 표시한다.

---

## 1. 한계 — 지금 구조가 못 하는 것

### 1-1. 런타임 스폰 오브젝트는 주입을 못 받는다

`AttributeInjector`는 씬의 `Awake`에서 1회 돌고 끝난다(`AttributeInjector.cs:27`).
씬이 뜬 뒤에 `Instantiate`로 만든 오브젝트는 `[Inject]` 필드가 null로 남는다.

현재 우회 방식:

| 위치 | 방식 |
|---|---|
| `Enemy.cs:158` `InitCharacterRefs` | 스포너가 자기 몫으로 받은 참조를 스폰 직후 넘긴다 |
| `WisuSpawnPhase.cs:12` | 스포너가 `[Inject(true)]`로 먼저 받아두고 스폰 개체에 전달 |

**문제:** 스폰 개체가 새 의존을 하나 추가할 때마다 스포너의 필드와 전달 메서드 인자를 같이 늘려야 한다.
`InitCharacterRefs(provider, notifier)`처럼 인자가 계속 늘어나는 형태가 된다.

**다음에 할 것:** `InjectTo(object target)` 같은 공개 진입점을 두고, 스폰 직후 한 줄로 호출한다.
스포너가 무엇을 넘길지 알 필요가 없어진다.

---

### 1-2. 씬을 넘길 상태가 static 5곳에 흩어져 있다

씬 전환 시 값을 유지하려고 `static` 필드를 쓴다. `DontDestroyOnLoad`를 안 쓰는 이유는 아래 2-4에 있다.

| 위치 | 값 | 언제 넣고 언제 꺼내나 |
|---|---|---|
| `PlayerHealth._carriedCurrent` | 체력 | `OnDestroy`에 넣고 다음 씬 `Awake`에서 꺼냄 |
| `EventSceneLoad._pendingHp` / `_pendingType` | 체력, 캐릭터 | `LoadNow`에 넣고 다음 씬 `GameStateSaver.Start`에서 꺼냄 |
| `SlotLoadRunner._pending` | 세이브 데이터 전체 | 씬 띄우기 직전에 넣고 `sceneLoaded`에서 꺼냄 |
| `SlotLoadRunner.LoadedFromSlot` | 불러오기로 들어왔는지 | 씬 진입 방식 판별 |
| `SaveSlotManager._currentSlot` | 지금 쓰는 슬롯 번호 | 게임 내내 유지 |

**공통 비용:** 다섯 곳 모두 `[RuntimeInitializeOnLoadMethod]`로 값을 손수 비우는 코드를 따로 갖는다.
도메인 리로드를 끈 에디터에서 지난 판 값이 딸려오기 때문이다. 같은 코드가 다섯 번 반복된다.

**판단이 갈리는 부분:** static은 "값만 남기면 충돌이 없다"는 점에서 맞는 선택이었다.
다만 씬을 넘길 값이 늘어날수록 어디에 무엇이 남는지 추적이 어려워진다. 5개가 한계에 가깝다.

**다음에 할 것:** 씬을 넘기는 값을 한 곳(예: `SceneTransitionContext`)에 모으고, 비우는 코드도 그 한 곳에만 둔다.

---

### 1-3. 같은 타입 구현이 2개 이상이면 조용히 어긋난다

`AttributeInjector.cs:196` — 같은 타입이 여러 개면 첫 번째만 주입하고 `LogWarning`만 남긴다.
경고를 놓치면 엉뚱한 인스턴스가 꽂힌 채로 게임이 돈다.

**다음에 할 것:** 여러 개일 때 어느 쪽을 쓸지 지정할 방법(이름표, 명시적 우선순위)이 없으면
아예 예외로 멈추는 편이 추적하기 쉽다.

---

### 1-4. 주입 시점이 `Awake` 하나뿐이다

주입은 씬 `Awake`에서 끝난다. 그래서:

- `Awake` 안에서 다른 컴포넌트의 `[Inject]` 필드를 읽으면 아직 안 들어와 있을 수 있다.
- 실행 순서를 `Script Execution Order`에서 `-100`으로 고정해야만 성립한다(2-1).

`PlayerHealth.Awake`가 `Setup`을 먼저 부르는 것도 이 제약 때문이다.
다른 컴포넌트가 `Start`에서 `Current`를 읽어도 값이 차 있게 하려고 `Awake`에 뒀다.

---

## 2. 주의사항 — 알고 써야 하는 것

### 2-1. Script Execution Order = -100

`AttributeInjector`의 실행 순서를 `-100`으로 두지 않으면 주입 전에 다른 `Awake`가 먼저 돈다.
프로젝트 설정에 들어 있고 코드로 강제되지 않는다. 설정이 날아가면 원인을 찾기 어렵다.

### 2-2. 부모 클래스의 `[Inject]` 필드는 `protected`

`AttributeInjector.CollectInjectFields`가 자식 클래스 기준으로 필드를 훑는다.
부모에 `private`로 두면 스캔에 안 잡혀 **경고 없이** null로 남는다.
`SettingsHolder._saveService`가 `protected`인 이유다.

### 2-3. Enum 순서

씬·프리팹·SO에 저장되는 enum은 이름이 아니라 정수로 직렬화된다.
중간에 항목을 끼우면 이미 배치된 값이 전부 밀린다. 자세한 건 `Docs/EnumGuide.md`.

`EffectId.cs:5`에 같은 문제 인식이 TODO로 남아 있다. 값 명시(`= 1`, `= 2`)가 안 된 enum이 아직 있다.

### 2-4. `DontDestroyOnLoad`를 쓰지 않는 이유

씬마다 `Static` 프리팹이 놓인다. 오브젝트를 `DontDestroyOnLoad`로 살려두면
살아남은 옛것과 새 씬의 사본이 **둘 다** 주입 대상으로 등록된다 → 1-3의 "첫 번째만 주입" 상황이 된다.

`SaveManager`, `SaveSlotManager`, `SlotLoadRunner` 주석에 같은 설명이 세 번 반복돼 있다.
규칙이 코드로 강제되지 않고 주석으로만 존재한다는 뜻이다.

---

## 3. 독립성 점검 — 시스템별 결합도

### 3-1. 판정 기준

- **독립**: 다른 시스템을 참조하지 않거나, 인터페이스로만 참조한다.
- **새는 곳**: 다른 시스템의 **구체 클래스**를 `[Inject]`로 직접 받는다.

### 3-2. 시스템별 현황

| 시스템 | 외부 의존 | 판정 |
|---|---|---|
| `GameStateSystem` | 없음 (`IGameStateProvider`는 자기 것) | 독립 |
| `LanguageSystem` | `ILanguageSettings` | 독립 |
| `CombatSystem` | `HitChannel`(SO) | 독립 |
| `InputSystem` | `IGameStateProvider`, `IInputKeySettings`, `InputActionAsset` | 독립 |
| `SettingsSystem` | `ISaveService` | 독립 |
| `AudioSystem` | `ISoundSettings` + 자기 SO | 독립 |
| `CameraSystem` | 인터페이스 다수 + `PlayerCameraShakeData`(자기 SO) | 대체로 독립 |
| `PlayerSystem` | 인터페이스 다수 + `AudioChannel`·`HitChannel`·`EffectCatalog`(SO) | 대체로 독립 |
| `UISystem` | 인터페이스 다수 + `ButtonClickSound`, `CinemachineInputAxisController` | 일부 새는 곳 |
| `SaveSystem` | `PlayerPartner`, `SaveSlotManager` | 새는 곳 있음 |
| `GimmickSystem` | `SaveSlotManager`, `SlotLoadRunner`, `CameraRole`, `CinemachineCamera` | **가장 많이 샘** |
| `EnemySystem` | `public` 필드 직접 노출, `EnemyData`에 `CreateAssetMenu`도 없음 | 리팩터링 전 영역 |

### 3-3. 새는 곳 전체 목록 (코드에 TODO로 표시된 것)

| 위치 | 무엇을 직접 참조 | 있어야 할 인터페이스 |
|---|---|---|
| `EventBossCamera.cs:21` | `List<CameraRole>` | CameraSystem의 카메라 전환 인터페이스 |
| `EventCameraStop.cs:13` | `List<CameraRole>` | 같음 |
| `EventStartView.cs:14` | `List<CameraRole>` | 같음 |
| `EventFallRespawn.cs:12,14` | `SaveSlotManager`, `SlotLoadRunner` | SaveSystem의 슬롯 조회·로드 인터페이스 |
| `EventSaveGame.cs:22` | 저장 구현 | SaveSystem의 저장 인터페이스 |
| `EventSceneLoad.cs:26` | 저장 구현 | 같음 |
| `GameStateSaver.cs:15` | `PlayerPartner` | PlayerSystem의 파트너 상태 인터페이스 |
| `LockOnMarker.cs:18` | `LockOnController` | `ILockOnState`·`ILockOnTarget` |

**공통 원인:** 인터페이스가 없어서가 아니라, **CameraSystem·SaveSystem이 아직 자기 진입점을 확정하지 않아서**다.
`Docs/SystemUsageGuide.md`에서 두 시스템 모두 상태가 `미확정`이다.
확정되지 않은 시스템에 붙는 쪽은 구체 클래스를 잡을 수밖에 없다.

### 3-4. 미확정 시스템

`SystemUsageGuide.md` 기준 확정 4개: DISystem, AudioSystem, GameStateSystem, InputSystem.
미확정 9개: CameraSystem, CombatSystem, EnemySystem, GimmickSystem, LanguageSystem, PlayerSystem, SaveSystem, SettingsSystem, UISystem.

> 3-2의 "독립" 판정은 결합도 기준이고, 여기 "확정/미확정"은 가이드 항목 작성 여부다. 축이 다르다.

미확정 상태에서 다른 시스템이 그 시스템에 의존하면 3-3 같은 결합이 계속 생긴다.
**확정 순서가 결합도를 결정한다.**

---

## 4. 흐름이 부자연스러운 곳과 원인

### 4-1. 체력이 씬을 넘는 경로가 3개다

같은 값(플레이어 체력)이 세 경로로 전달되고, 실행 순서로 서로를 덮는다.

| 순서 | 경로 | 시점 | 조건 |
|---|---|---|---|
| 1 | `PlayerHealth._carriedCurrent` | `Awake` | 항상 |
| 2 | `SlotLoadRunner._pending` → `GameStateSaver.Restore` | `sceneLoaded` | 불러오기로 진입 시 |
| 3 | `EventSceneLoad._pendingHp` → `GameStateSaver.Start` | `Start` | `LoadedFromSlot`이 false일 때만 |

**결과:** 1번은 2번·3번이 있는 경로에서 항상 덮인다. 실질적으로 쓰이는 건 기믹을 거치지 않는 씬 전환뿐이다.

**원인:** 세 경로가 각각 다른 시점에 다른 시스템에서 추가됐다.
`PlayerHealth`는 "체력의 단일 출처"라고 주석에 적혀 있지만, 실제로 체력을 씬 너머로 옮기는 주체가 셋이다.

**정리 방향:** 씬 진입 시 체력을 정하는 주체를 하나로 정한다.
`GameStateSaver`가 진입 방식(새 게임 / 지역 이동 / 불러오기)을 판별해 한 번만 값을 넣는 형태가 자연스럽다.
`PlayerHealth`는 값을 보관·증감만 하고 씬 전환을 모르게 한다.

### 4-2. `SaveSystem`이 `GimmickSystem`의 static을 직접 부른다

`GameStateSaver.cs:24`가 `EventSceneLoad.TakePending(...)`을 호출한다.
저장 시스템이 기믹 시스템의 static 메서드에 의존한다.

**원인:** 값을 넘기는 쪽이 기믹(`EventSceneLoad`)이라 저장할 데이터가 기믹 쪽에 놓였다.
`SceneManager.LoadScene`에 인자를 실을 수 없다는 유니티 제약이 출발점이다.

**정리 방향:** 4-1과 같다. 씬 전환 데이터를 담는 곳이 SaveSystem 쪽에 있으면 이 호출 방향이 뒤집힌다.

### 4-3. 낙사 복귀가 저장 시스템을 경유한다

`EventFallRespawn`은 현재 체력을 **세이브 슬롯에 덮어쓰고** 씬을 다시 띄운다.

**부작용:**
- 낙사할 때마다 세이브 파일이 쓰인다.
- 사용자가 "저장한 적 없는데 저장됐다"고 느낄 수 있다.
- `EnterTrigger`의 `Is Loop`를 켜야 한다는 배치 제약이 붙는다.

**원인:** 씬 재시작 + 상태 복원 기능이 저장/불러오기에만 있어서 그걸 재사용했다.

**정리 방향:** "체크포인트 복원"과 "세이브 파일 저장"을 분리한다. 전자는 메모리에만 있으면 된다.

### 4-4. 컷씬 스킵이 static 인스턴스를 쓴다

`CutSceneSkipTrigger.Active`가 `public static`이다.
"컷씬은 동시에 하나뿐"이라는 전제를 static으로 표현했다.

**위험:** 전제가 깨지는 순간(컷씬 2개 동시 재생) 조용히 어긋난다. 컴파일도 되고 경고도 없다.

**정리 방향:** 전제를 유지하려면 현재 활성 컷씬을 관리하는 주체를 명시적으로 두거나,
`GameStateSystem`의 `Cutscene` 상태에 그 정보를 붙인다.

### 4-5. `AudioSystem`을 거치지 않는 소리가 있다

BGM과 적 소리는 씬/프리팹의 `AudioSource`를 직접 쓴다(`SystemUsageGuide.md`에 예외로 명시).

**결과:** 볼륨 설정이 이 소리들에 적용되는 경로가 다르다. 소리 수치를 고칠 때 봐야 할 곳이 두 군데가 된다.

**원인:** 적 시스템이 리팩터링 대상 밖이었다. 판단이었지 실수는 아니지만, 규칙에 구멍이 남았다.

### 4-6. `StateData` 한 클래스에 카테고리 15개

`StateData`가 `InputBlock`부터 `ObjectToggle`까지 15종 배열을 필드로 갖는다.
카테고리를 추가하려면 필드 선언, `BuildDataMap` 등록, enum 추가를 세 곳에 손대야 한다.

**완화되어 있는 부분:** 커스텀 에디터로 안 쓰는 항목을 인스펙터에서 가린다. 사용자 입장의 복잡도는 낮다.

**남는 문제:** 코드 쪽 추가 비용이 카테고리 수에 비례한다.

**판단이 갈리는 부분:** 이 구조는 "상태마다 연출을 한 곳에서 조정"이라는 목적을 달성하고 있다.
카테고리가 20개를 넘어가면 재검토, 그 전까지는 그대로 두는 게 싸다.

### 4-7. `Enemy`가 `public` 필드로 되어 있다

`Enemy.cs` 상당수 필드가 `public`이다(`hp`, `animator`, `target`, `isDead` …).
`[HideInInspector]`로 인스펙터에서만 가렸을 뿐, 코드에서는 아무나 쓸 수 있다.

**원인:** 이 시스템은 리팩터링 대상이 아니었다.

**영향:** 적 관련 규칙을 세울 수 없어 `SystemUsageGuide.md`의 `EnemySystem`이 미확정으로 남아 있다.

---

## 5. 다음 프로젝트에 고민할 것

### 5-1. DI 진입점을 3개로 정한다

지금은 `Awake` 1회뿐이다. 최소 이 세 개가 있어야 우회 코드가 사라진다.

| 진입점 | 언제 |
|---|---|
| 씬 주입 | 씬 `Awake` (지금 있는 것) |
| 객체 주입 `InjectTo(object)` | 런타임 스폰 직후 |
| 해제 | 객체 파괴 시 등록 해제 |

### 5-2. 씬을 넘길 상태를 한 곳에 모은다

지금 5곳에 흩어진 static을 하나의 컨텍스트로 모으고, 도메인 리로드 대비 초기화도 그 한 곳에서만 한다.
"무엇이 씬을 넘어가는가"를 파일 하나만 열면 알 수 있게 한다.

### 5-3. enum은 시작 시점에 값을 명시한다

`Docs/EnumGuide.md`에 이미 적혀 있는 대로, **프로젝트 시작 시점에** 번호대를 잡아야 한다.
배치가 끝난 뒤에는 되돌리는 비용이 씬·프리팹 전체 재지정이다.

### 5-4. 시스템 확정 순서를 먼저 정한다

3-3의 결합은 전부 "의존받는 쪽이 미확정"이라 생긴다.
**많이 참조되는 시스템부터 진입점을 확정한다.** 참조 수 기준으로는 SaveSystem, CameraSystem, PlayerSystem 순이다.

### 5-5. 주석으로만 있는 규칙을 코드로 옮긴다

다음 세 가지가 지금 주석에만 있다.

| 규칙 | 지금 |
|---|---|
| `DontDestroyOnLoad` 쓰지 않기 | 주석 3곳에 반복 |
| 부모 `[Inject]` 필드는 `protected` | 주석 1곳 |
| Script Execution Order `-100` | 주석 1곳 + 프로젝트 설정 |

에디터 검사(빌드 전 스크립트)로 위반을 잡으면 주석 반복이 사라진다.

### 5-6. "저장"과 "복원"을 분리한다

4-3의 원인이다. 파일에 쓰는 저장과, 메모리에 들고 있다 되돌리는 복원은 다른 기능이다.
처음부터 나누면 낙사 복귀가 세이브 파일을 건드리지 않는다.

### 5-7. 리팩터링 대상 밖 영역을 명시한다

`EnemySystem`, 보스, BGM은 대상 밖이었다. 이건 판단이지 실수가 아니다.
다만 **대상 밖이라는 사실이 문서 한 곳에 적혀 있어야** 규칙 위반과 구분된다.
지금은 `SystemUsageGuide.md` 안에 "예외 현황" 한 줄로만 있다.

### 5-8. 내가 만든 것만 따로 저장소로 뽑는다

지금 저장소는 팀 작업물과 외부 에셋(`Assets/4.Plug-in`)이 섞여 있다.
내 기여만 보여주려면 별도 저장소가 필요하다. 옮길 때 기준:

- **넣는다** — 내가 쓴 코드(`Assets/1.Code`), 내가 만든 셰이더·애니메이션·프리팹·SO, 문서(`Docs/`), 버전 기록
- **뺀다** — 구매·무료 에셋 원본, 남이 만든 산출물, `Library/` 같은 생성물
- **대체한다** — 에셋이 빠져 안 돌아가는 씬은 스크린샷·영상과 설명으로 대신한다

분리 자체는 판단이다. 다만 **나중에 뽑으려면 지금부터 커밋을 내 작업 단위로 끊어 두는 게 싸게 먹힌다.**

### 5-9. 코드 밖 작업도 같은 무게로 기록한다

이 문서는 지금까지 C# 구조 얘기만 한다. 그런데 저장소에는 코드 밖 작업물이 같이 있다.

- 셰이더 — `Assets/1.Code/Shaders/OutlineSilhouette.shader`, 셰이더 그래프 다수(`Assets/3.Content/**`)
- 애니메이션 — `.anim` 87개(직접 만든 것과 에셋에서 온 것이 섞여 있다)
- 그 외 — VFX, 포스트프로세싱 설정, UI 리소스, 사운드 배치

앞으로의 기록은 코드와 같은 식으로 남긴다:
**무엇을 왜 그렇게 만들었는가, 뭐가 안 됐는가, 다음엔 어떻게 할 것인가.**
내가 만든 것과 에셋을 가져다 쓴 것을 **반드시 구분해서** 적는다. 5-8의 분리 기준도 이 구분에 기대게 된다.

### 5-10. 문서는 웹페이지로 만들어 공개하는 쪽을 고려한다

이번에 시스템 평가 결과를 `Docs/시스템평가문제리스트.md`로 쓰고, 같은 내용을
한 장짜리 HTML(`Docs/시스템평가문제리스트.html`)로도 뽑아 봤다. 원본은 같은데 읽는 경험이 달랐다.

- md는 **순서대로 읽는** 문서다. 63건을 위에서 아래로 훑는 것 말고는 할 수 있는 게 없다.
- html은 **찾는** 문서다. 유형(조용한 실패·결합·버그)으로 걸러 보고, 클래스명으로 검색하고,
  "전체 63건 중 결합이 21건"처럼 분포가 첫 화면에서 바로 보인다.

문제 목록·이슈·커밋처럼 **항목이 많고 유형이 있는 자료**는 후자가 확실히 낫다.
읽는 사람이 자기가 궁금한 축으로 자를 수 있기 때문이다.

다음 프로젝트에서 고려할 것:

- **만드는 비용이 생각보다 싸다.** 데이터를 배열로 두고 필터 UI만 붙이면 파일 하나로 끝난다.
  라이브러리도 빌드도 필요 없고, 더블클릭하면 브라우저에서 열린다.
- **자동 수집과 붙이면 더 싸진다.** `git log --format=...`, `gh issue list --json ...`으로 뽑은 값을
  그대로 데이터 배열에 넣으면 된다. 커밋 히스토리나 이슈 현황도 같은 틀로 만들 수 있다.
- **한계는 스냅샷이라는 점이다.** 뽑은 시점의 값이라 커밋이 쌓이면 다시 돌려야 한다.
  자동 갱신이 필요하면 GitHub Actions로 push마다 다시 생성하게 하거나, 그게 과하면
  "언제 기준 자료인지"를 페이지에 적어 두는 선에서 끝낸다.
- **공개 여부는 따로 판단한다.** 이 저장소는 public이라 `Docs/`에 넣고 push하면 그대로 공개된다.
  GitHub Pages를 켜면 링크로 바로 열리는 페이지가 되므로 포트폴리오로 쓰기 좋다.
  다만 문제 목록처럼 **내부 진단 성격의 자료는 공개할지 먼저 정하고** 커밋한다.

정리하면, **결과물을 md로만 남기지 말고 "읽는 사람이 누구인가"를 보고 형식을 고른다.**
나만 보면 md로 충분하고, 남에게 보여줄 자료면 웹페이지가 값을 한다.


### 5-11. 시스템은 오픈소스 프로젝트를 먼저 읽고 짠다

이번 프로젝트는 시스템 경계를 코드를 쓰면서 정했다. 그래서 1장·4장의 문제(static 흩어짐,
시스템 간 직접 호출)가 다 사후에 발견됐다. 다음엔 **짜기 전에 공개 코드를 읽고 경계부터 정한다.**

Unity·C#

- [bright-souls](https://github.com/leotgo/bright-souls) — 소울라이크. 상태·스태미나·적 AI라 이 프로젝트와 도메인이 겹친다
- [AnyRPGCore](https://github.com/AnyRPG/AnyRPGCore) — RPG 엔진. 시스템 수가 많아 경계 나누는 방식을 비교하기 좋다
- [daggerfall-unity](https://github.com/Interkarma/daggerfall-unity) — 완성된 상용급 규모. 큰 프로젝트가 어디서 갈라지는지 본다

Unreal·C++

- [ActionRoguelike](https://github.com/tomlooman/ActionRoguelike) — 액션·AI·세이브·비동기 로딩을 한 프로젝트에 담고 있다
- [EpicSurvivalGame](https://github.com/tomlooman/EpicSurvivalGame) — 챕터별 브랜치라 기능이 붙는 순서를 그대로 볼 수 있다
- [Bomber](https://github.com/JanSeliv/Bomber) — C++ 기반에 데이터 주도 설계. 멀티플레이 포함

읽는 방법은 이번과 같이 한다: 관계도를 먼저 그려서(5-10) 어디가 어디를 부르는지 보고,
**내 프로젝트의 시스템 목록과 나란히 놓고 차이를 적는다.** 베끼는 게 아니라

### 5-12. 자동 테스트를 어디까지, 언제 넣을 것인가

이번 프로젝트에 자동 테스트는 하나도 없다. 확인은 전부 에디터에서 직접 플레이해서 했다.
그래서 2장의 함정(실행 순서, `protected` 누락, enum 밀림)은 **증상이 나타난 뒤에야** 원인을 역추적했다.

#### 잡을 수 있는 층이 셋이다

| 층 | 무엇을 잡나 | 이 프로젝트에서 해당하는 것 |
|---|---|---|
| 에디터 정적 검사 | 규칙 위반. 실행 없이 에셋·코드를 훑어서 본다 | 5-5의 세 규칙, `[Inject]` 대상 부재, enum 값 미명시(2-3), `CreateAssetMenu` 없는 SO |
| PlayMode 테스트 | 상태 전이·수치. 씬을 띄우고 코드로 조작한다 | 4-1의 체력 경로 3개, 상태 머신 전이, 세이브→로드 왕복 |
| 자동 플레이 | 사람이 조작해야만 나오는 것 | B-1의 지형 관통·코너 끼임·낙사, 프레임 저하 |

아래로 갈수록 잡는 범위가 넓고, 만드는 비용과 **실패했을 때 원인을 찾는 비용**이 같이 커진다.

#### 층별로 무엇이 필요한가

**정적 검사** — 유니티 API만으로 된다. `AssetDatabase`로 프리팹·SO를 훑고, 리플렉션으로 `[Inject]` 필드를 모아
대응 구현이 씬에 있는지 센다. 이미 `AttributeInjector`가 런타임에 하는 일과 같은 일을 실행 전에 하는 것이다.
`TextureCompressTool`(A-4)처럼 에디터 스크립트 하나면 끝난다.

**PlayMode 테스트** — Unity Test Framework(NUnit 기반, EditMode/PlayMode 두 종류)를 쓴다.
다만 **테스트가 대상을 만들 수 있어야** 성립한다. 지금은 주입이 씬 `Awake`에 묶여 있어(1-4)
`PlayerHealth` 하나만 떼어 만들 수가 없다. 5-1의 `InjectTo(object)`가 있으면 테스트가 직접 조립할 수 있다.
**즉 5-1은 DI 편의가 아니라 테스트 가능성의 전제다.**

**자동 플레이** — 입력을 코드로 넣고 결과를 판정한다. 판정 기준이 있어야 한다.
"플레이어 y좌표가 지형 아래로 내려갔다", "N초간 위치 변화가 0인데 이동 입력이 들어와 있다"처럼
**수치로 쓸 수 있는 실패 조건**만 자동으로 잡힌다. "어색하다", "재미없다"는 못 잡는다.
입력은 이미 `InputSystem`으로 한 곳을 지나므로, 그 지점에 가짜 입력을 넣는 경로를 만드는 게 출발점이다.

#### 언제 넣는 게 맞나

층마다 답이 다르다.

| 층 | 시점 | 이유 |
|---|---|---|
| 정적 검사 | **규칙이 생긴 그 시점** | 규칙을 주석에 적는 대신 검사로 적는다. 나중에 넣으면 이미 쌓인 위반부터 치워야 한다 |
| PlayMode 테스트 | **시스템 진입점을 확정한 뒤**(5-4) | 진입점이 바뀌면 테스트가 통째로 깨진다. 미확정 9개에 지금 붙이면 테스트가 리팩터링을 막는다 |
| 자동 플레이 | **이동·충돌 구조를 한쪽으로 통일한 뒤**(B-1의 A안/B안) | 지금은 관통이 버그가 아니라 구조의 결과다. 구조를 안 정하고 검사부터 만들면 매번 실패만 뜬다 |

**공통 원칙:** 자동 테스트는 **정답이 확정된 것**만 검사할 수 있다.
설계가 흔들리는 동안 쓴 테스트는 자산이 아니라 부채가 된다.
반대로 규칙(정적 검사 대상)은 정해진 순간부터 안 바뀌므로 미룰 이유가 없다.

#### 다음 프로젝트에서 실제로 할 것

1. 규칙을 하나 정할 때마다 검사 스크립트를 같이 쓴다. 문서에만 적지 않는다.
2. DI에 `InjectTo(object)`를 처음부터 넣는다(5-1). 테스트가 대상을 조립할 수 있게 하려는 목적이 절반이다.
3. 세이브→로드 왕복처럼 **값이 같아야 한다**로 표현되는 것부터 PlayMode 테스트로 만든다. 판정이 명확해서 싸다.
4. 자동 플레이는 마지막이다. 그 전에 "실패"를 수치로 정의할 수 있는지부터 확인한다.

**정할 수 없는 부분:** 이 프로젝트에 지금 소급해서 넣을지는 별개 판단이다.
미확정 9개가 남아 있으므로 2·3번은 지금 붙여도 곧 깨진다. 1번(정적 검사)만 지금 값을 한다.

"저긴 왜 여기서 잘랐나"를 확인하는 용도다.

---

## 부록 A. 코드에서 옮겨온 TODO

`Assets/1.Code`에 있던 `대원_TODO` / `TODO:` 주석을 여기로 모았다. **원본 주석은 지웠다.**
아래 위치는 옮길 당시의 줄 번호다.

### A-1. 시스템 경계 (3-3과 같은 항목)

| 원래 위치 | 내용 |
|---|---|
| `EventBossCamera.cs:21` | 시스템 간 참조는 인터페이스로 받는다. CameraSystem이 카메라 전환 인터페이스를 내놓고 그걸 받도록 바꾼다 |
| `EventCameraStop.cs:13` | 위와 같음 |
| `EventStartView.cs:14` | 위와 같음 |
| `EventFallRespawn.cs:12` | SaveSystem이 슬롯 조회·로드 인터페이스를 내놓고 그걸 받도록 바꾼다 |
| `EventFallRespawn.cs:14` | 위와 같음 |
| `EventSaveGame.cs:22` | SaveSystem이 저장 인터페이스를 내놓고 그걸 받도록 바꾼다 |
| `EventSceneLoad.cs:26` | 위와 같음 |
| `GameStateSaver.cs:15` | PlayerSystem이 파트너 상태 인터페이스를 내놓고 그걸 받도록 바꾼다 |
| `LockOnMarker.cs:18` | CameraSystem의 `ILockOnState`·`ILockOnTarget`으로 바꾼다 |

### A-2. 데이터 위치

| 원래 위치 | 내용 |
|---|---|
| `PlayerHealth.cs:7` | `MaxHealth`를 데이터 묶음에서 `Inject`로 받아오기 |
| `PlayerCharacterMover.cs:27` | 데이터 SO로 그룹화 |
| `PlayerMoveAnimation.cs:22` | 애니메이터 파라미터 이름을 플레이어 데이터로 옮기기 |
| `EffectId.cs:5` | enum이 에셋에서 사용되면 항목 추가 시 밀리는 문제가 발생한다. 어찌해야 하는가 |

### A-3. 미구현·확인 필요

| 원래 위치 | 내용 |
|---|---|
| `WisuMainRe.cs:496` | 보스를 죽이면 보스를 잡았다는 UI 등장 |
| `FootstepEmitter.cs:19` | 모바일 이동 입력이 `MoveX`, `MoveY`에 어떻게 들어오는지 확인 필요 |
| `MenuInputHandler.cs:15` | 메뉴에선 이동 입력을 쓰지 않는다. 다만 조작키 변경 시 모든 키를 받는 부분은 생각해봐야 한다 |
| `HitVignetteEffect.cs:12` | `HitState`(SO) > Effect > Hit Vignette의 Duration 주의사항. `Duration > _fadeInTime + _holdTime + _fadeOutTime` 이어야 한다 |

### A-4. 도구 확장안

`TextureCompressTool.cs:71` — EditorWindow 형태의 텍스처 관리 툴로 확장

- 수집 버튼: 프로젝트 전체 텍스처(또는 씬에서 실제 참조되는 텍스처만) 목록화
- 표 형태 표시: 행=텍스처, 열=플랫폼별(Default/Android) MaxSize·Format·Compression
- 일괄 적용 버튼: 플랫폼별 설정을 한 번에 적용
- 예외 체크박스: 체크된 텍스처는 일괄 적용에서 제외
- 신규 임포트 텍스처는 툴로 못 잡으므로 `AssetPostprocessor` 병행 필요

---

## 부록 B. 폐기된 코드(`Old/`)에 남아 있던 기록

주석 처리된 채 `Old/` 폴더에 남아 있던 시도들이다. 다시 손댈 때 참고할 값이 있어 옮긴다.
**원본 주석은 지웠다.**

### B-1. 지형 관통 문제와 센서 구조 (`Old/PlayerSensor.cs`)

**배경 (`대원_Insite`)**
높은 속도로 이동(`Rigidbody.MovePosition`은 사실 transform 이동)할 때 캐릭터가 지형을 뚫고 지나간다.
이를 해결하려고 이동 전에 충돌을 검사하는 Sensor를 구현했다. AI로 충돌 방지 코드를 작성했으나 여러 상황에서 지형이 뚫렸다.
**이럴 때는 사용자가 문제를 이해하고 원인을 짚어주고 해결책을 제시하는 게 좋을 것 같다.**

**구현 내용**
이동 컴포넌트가 넘긴 이동량만큼 플레이어의 실제 `CapsuleCollider` 모양으로 캡슐을 휩쓸어(`CapsuleCast`) 충돌을 검사한다.

- 캐스트 캡슐은 콜라이더 치수를 그대로 쓰되 스킨만큼 작게 한다(콜라이더보다 크면 안 됨).
- 검사 출발점을 이동 반대로 백오프해 출발 시 겹침으로 충돌을 놓치는 문제를 줄인다.
- 충돌하면 닿기 직전까지만 전진하고 남은 이동량을 충돌면에 흘린다. 면 법선(뚫는) 방향만 제거하므로 위아래 이동은 살아남고(벽 타고 오르기 가능) 지형 관통은 막힌다.

**미해결 문제 (센서 전담 구조는 시도했다가 되돌림)**

| 문제 | 내용 |
|---|---|
| **구조 간섭 (가장 근본)** | 캐릭터가 일반 Rigidbody(중력 켜짐) + 수동 이동(`MovePosition` + 센서)을 동시에 굴려 서로 간섭한다. 빠른 이동은 RB 콜라이더(`CollisionDetection=Discrete`)가 못 막아 뚫리고, 코너에선 콜라이더가 끼인다. |
| | 해결하려면 충돌 책임을 한쪽으로 통일해야 한다. **(A)** RB를 kinematic으로 두고 이동·낙하·충돌을 전부 코드로, 또는 **(B)** `MovePosition`·센서를 버리고 `velocity`/`AddForce` + Continuous 충돌로 물리 엔진에 맡김. |
| | (A)를 시도하다 "이동 합산(수평+낙하+스킬을 한 곳에서 `MovePosition`)·낙하 직접 구현"까지 갔으나 **평지 버벅임으로 보류**함. |
| **공중 걷기** | 절벽의 90도 안쪽 코너에서 콜라이더가 두 벽에 끼이고 센서가 수평 이동을 0으로 만들어 제자리 고착 → `MovePosition`이 매 프레임 같은 y를 덮어써 중력 낙하가 무시됨 → 공중에서 걷는 판정. (일반 낭떠러지는 정상적으로 떨어짐) |
| **코너 관통** | 90도 코너에서 한 벽에 투영한 결과가 옆 벽을 향하는데, 결과를 재검사하지 않아(캐스트 1회) 옆 벽을 파고든다. → collide & slide 반복(iteration 2~3회) 필요. 비용은 충돌이 있을 때만 늘어난다. |
| **깊은 시작 겹침** | 이미 `_backOff`보다 깊이 박힌 채 출발하면 `CapsuleCast`가 그 면을 시작 겹침으로 무시 → 통과. 백오프는 완화일 뿐. `Physics.ComputePenetration`(박힌 만큼 밀어내기)을 병행해야 완전하다. |
| **박힘 해소 없음** | 한번 지형에 박히면(이동량 0 포함) 빠져나오는 로직이 없어 박힌 채 고정될 수 있다. |
| **버벅임** | 센서 전담(kinematic)으로 갈 경우 `MovePosition`이 물리 스텝(기본 50Hz)에만 위치를 바꿔 보간 없으면 끊겨 보인다. → `Rigidbody Interpolate=Interpolate` 필요, 카메라 추적 타이밍(`LateUpdate`)도 함께 점검. |

### B-2. CharacterController 이동 시도 (`Old/CharacterControllerPlayerMover_Test.cs`)

씬 테스트 준비 절차로 남아 있던 메모다.

- 캐릭터(H·A) 오브젝트에 `CharacterController`를 추가하고 Radius/Height/Center를 캡슐에 맞춘다.
- 이 컴포넌트는 기존 매니저 오브젝트(`PlayerMovement`가 있던 곳)에 두면 DI로 입력·상태·현재 캐릭터가 주입된다.
- 기존 `PlayerMovement` / `PlayerSkillMoveHandler`는 같은 캐릭터를 두 번 움직이지 않도록 꺼두고 테스트한다.
  **(TODO)** 기존 두 컴포넌트와 동시 작동하지 않게(중복 이동 방지) 씬에서 켜고 끄는 것을 잊지 말 것.

### B-3. 스킬 이동 확장안 (`Old/PlayerSkillMoveHandler.cs`)

- **주변 감지로 이동량 변화 확장 예정** — 벽 앞에서 돌진 거리를 줄이는 식.
- **물리 이동이 의도한 모양대로 나올까?** — 속도를 플레이어 방향으로 회전시켜 합산하는 방식에 대한 의문.

### B-4. 인터페이스 주입 방식 폐기 (`Old/IInterfaceInjected.cs`)

구현체가 `Dictionary<Type, List<object>> injectedImplements`를 직접 들고 있어야 하는 방식이었다.

> 해당 방식은 구현체에서 작성해야 할 코드에 부담을 줌. 설계가 적절한지 다시 판단하기

현재의 `[Inject]` 어트리뷰트 방식으로 대체됐다. 판단은 끝난 항목이다.

### B-5. 이동값 하드코딩 (`Old/PlayerMovement.cs`)

`private float _moveRate = 10;` — 플레이어 데이터로 옮기기.
A-2의 `PlayerCharacterMover.cs:27`과 같은 항목이다.

---

## 부록 C. 학습 메모

할 일이 아니라 그때 이해한 것을 적어둔 기록이다. **원본 주석은 지웠다.**

### C-1. 각도와 라디안 (`FootstepEmitter.cs:67`)

- 둘레/지름이 항상 일정하다는 발견. 둘레/지름 = π. 둘레는 2πr.
- 호의 길이(s)가 반지름(r)과 똑같아지는 순간의 벌어진 각도 = **1 라디안**.
- 한 바퀴면 2πr/r = 2π 라디안. 즉 360도 = 2π 라디안.
- `Atan2`는 라디안을 반환하므로 `180 / π`(= `Mathf.Rad2Deg`)를 곱해 각도로 변환한다.
- 8방향 걷기 애니메이션이 있어서 좌표보다 각도로 구분하는 편이 맞다.

### C-2. 평면 투영 (`GravityVelocitySource.cs:42`)

`Vector3.ProjectOnPlane(vector, planeNormal)` — `planeNormal`이 법선인 평면에 `vector`를 내린 것.
가파른 경사에서 쌓인 중력 속도를 경사면을 따라 흐르게 만들 때 쓴다.

### C-3. 상태 이벤트 발행에 `while`을 쓴 이유 (`Old/CharacterBaseState.cs:73`)

`if`문으로 진행할 경우 프레임마다 실행하기 때문에 의도된 결과가 나오지 않는다.
한 프레임에 여러 이벤트 지점을 지나칠 수 있으므로 `while`로 밀린 것을 모두 소비해야 한다.

---

## 부록 D. 언젠가 해볼 것

지금 당장 필요한 건 아니고, 해두면 편해질 것 같아서 적어둔다.

### D-1. 검사만 맡는 에이전트

작업을 끝냈을 때 그 변경분을 옆에서 한 번 봐줄 누군가가 있으면 좋겠다는 얘기다.
세션을 하나 더 켜서 "이거 좀 봐줘" 하는 건 매번 번거롭고,
팀메이트로 붙이면 세션마다 자기 대화 전체를 계속 다시 들고 가야 해서 토큰이 아깝다.
그래서 필요할 때만 잠깐 떴다 사라지는 서브에이전트가 이 일에 제일 맞아 보인다.
스킬로 만들어두면 부를 때마다 같은 기준으로 봐줄 것이다.

### D-2. 왜 이렇게 짰는지 적어두는 파일

코드 주석은 꼭 필요한 것만 남기기로 했으니, 나머지 이야기를 둘 곳이 하나 필요하다.
파일을 고칠 때마다 "이건 무슨 기능이고, 무엇 때문에 이렇게 됐는지"를 md에 자동으로 쌓아두는 것.
나중에 커밋 메시지를 쓸 때 기억을 더듬거나 diff를 다시 읽지 않아도 되는 게 목적이다.
