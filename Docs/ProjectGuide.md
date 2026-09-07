# 프로젝트 가이드 (비개발자용)

이 문서는 **코드를 건드리지 않고** 게임을 만지는 사람을 위한 설명서다.
"어디에 무엇을 넣는지", "수치를 어디서 바꾸는지", "프리팹을 어떻게 배치하는지"만 다룬다.

> 코드를 쓰는 사람은 `Docs/SystemUsageGuide.md`를 본다. 이 문서와 대상이 다르다.

---

## 1. 폴더 지도 — 무엇을 어디에 넣나

프로젝트 파일은 `Assets/` 아래 번호가 붙은 폴더로 나뉜다. **번호 = 성격**이고, 파일을 새로 넣을 때는 아래 표대로 넣는다.

| 폴더 | 무엇이 들어가나 | 예 |
|---|---|---|
| `1.Code/` | 스크립트(코드). **비개발자는 건드리지 않는다.** | `Scripts/AudioSystem/…` |
| `2.Level/` | 씬과 레벨 배치용 프리팹 | `Scenes/Scene2.unity`, `LevelPrefabs/…` |
| `3.Content/` | 원본 리소스 — 모델, 텍스처, 애니메이션, 사운드 파일, UI 이미지, 포스트프로세싱 | `3.Content/Enemy/…`, `3.Content/UI/…` |
| `4.Plug-in/` | 외부에서 사온 에셋 | — |
| `5.Data/` | **수치 데이터(ScriptableObject).** 밸런스를 만지는 곳 | `5.Data/Enemy/EnemyMinionData.asset` |
| `Settings/`, `Plugins/` | 유니티/외부 설정. 건드리지 않는다 | — |

핵심 구분 두 가지만 기억하면 된다.

- **`3.Content` = 재료** (사운드 wav, 모델 fbx, 이미지 png)
- **`5.Data` = 수치** (그 재료를 얼마나 크게, 얼마나 빠르게 쓸지)

프리팹은 성격으로 가른다.

- 씬에 손으로 배치하는 것 → `2.Level/LevelPrefabs`
- 여러 곳에서 재료로 갖다 쓰는 것 → `3.Content`

`2.Level/LevelPrefabs` 안은 다시 이렇게 나뉜다.

| 폴더 | 내용 |
|---|---|
| `1.Systems/` | 씬에 반드시 있어야 하는 시스템 덩어리 (`Static`, `Camera`) |
| `2.Environment/` | 배경 오브젝트, 그림자 |
| `3.GamePlay/` | 플레이어, 적, 기믹 트리거 |
| `4.UI/` | HUD, 메뉴, 알림, 페이드 |

---

## 2. 새 씬을 만들 때 — 반드시 넣어야 하는 프리팹

빈 씬에는 아무것도 동작하지 않는다. 아래 프리팹을 **드래그해서 넣어야** 게임이 돌아간다.
(근거: 기존 씬 `Scene1`~`Scene5`에 공통으로 들어있는 목록)

### 2-1. 게임플레이 씬 필수 목록

| 순서 | 프리팹 | 위치 | 없으면 생기는 일 |
|---|---|---|---|
| 1 | **Static** | `2.Level/LevelPrefabs/1.Systems/` | 아무것도 동작 안 함. 소리·입력·저장·게임상태 전부 죽음 |
| 2 | **Camera** | `1.Systems/` | 화면이 안 나옴. 락온도 안 됨 |
| 3 | **Player** | `3.GamePlay/Player/` | 조작할 캐릭터가 없음 |
| 4 | **PlayerHUD** | `3.GamePlay/Player/` | 체력바가 안 보임 |
| 5 | **EventSystem** | `4.UI/` | UI 버튼이 안 눌림 |
| 6 | **InputPCHUD / InputMobileHUD** | `4.UI/HUD/` | 모바일 조작 버튼이 없음 |
| 7 | **PauseMenu** | `4.UI/Menu/` | ESC를 눌러도 메뉴가 안 열림 |
| 8 | **ScreenFade** | `4.UI/Notifications/` | 씬 전환이 뚝 끊김 |
| 9 | **AreaNotification** | `4.UI/Notifications/` | 지역 이름 안내가 안 뜸 |
| 10 | **PlayerDeathTrigger** | `3.GamePlay/Gimmick/` | 낙사해도 아무 일도 안 일어남 |
| 11 | **IndicatorTrigger** | `3.GamePlay/Gimmick/` | 진행 방향 안내가 안 나옴 |

메뉴 씬(`Scene0_MainMenu`)은 예외다. **Static + EventSystem + MainMenu** 세 개만 들어간다.

### 2-2. Static 프리팹 안에 뭐가 들어있나

건드릴 일은 거의 없지만, 무엇이 죽었는지 판단할 때 쓴다.

| 자식 오브젝트 | 하는 일 |
|---|---|
| `AttributeInjector` | 시스템끼리 연결해주는 배선 담당. **가장 중요.** 이게 없으면 전부 멈춤 |
| `SOContainer` | 5.Data의 수치 에셋을 게임에 물려주는 곳 (→ 3장) |
| `Audio` | 소리 재생, 볼륨 |
| `InputSystem` | 키보드·패드·모바일 입력 |
| `GameState` | 게임 중 / 컷씬 중 / 메뉴 중 구분, 일시정지, 마우스 커서 |
| `Save` | 저장·불러오기, 세이브 슬롯 |
| `Setting` | 볼륨·마우스감도·조작키·언어 설정 보관 |
| `Platform` | PC / 모바일 판별 |

### 2-3. 씬을 빌드에 등록하기

새 씬을 만들었으면 `File > Build Settings`에 등록해야 씬 이동이 된다. 현재 등록 순서는 이렇다.

| 번호 | 씬 |
|---|---|
| 0 | Scene0_MainMenu |
| 1 | Scene1_Tutorial |
| 2 | Scene2 |
| 3 | Scene3 |
| 4 | Scene4 |
| 5 | Scene5_Boss |

**주의:** 이 번호는 세이브 데이터에도 저장된다. **중간에 씬을 끼워넣어 번호가 밀리면 기존 세이브가 엉뚱한 씬을 연다.** 새 씬은 목록 맨 뒤에 붙인다.

---

## 3. Static 안 `SOContainer` — 수치 에셋 등록하는 곳

`5.Data`에 새 `.asset`을 만들어도, **`SOContainer`에 등록하지 않으면 게임이 그 파일의 존재를 모른다.**

1. 씬에서 `Static > SOContainer` 선택
2. 인스펙터의 `Groups` 목록에서 성격에 맞는 그룹을 편다 (Audio, Player, Enemy …)
3. `Assets` 리스트에 `.asset` 파일을 드래그해서 넣는다

그룹 구분은 **사람이 찾기 쉬우라고 나눈 것뿐**이고, 어느 그룹에 넣든 동작은 같다.

---

## 4. 수치를 어디서 바꾸나 — `5.Data` 전체 목록

수치를 바꿀 때는 **코드가 아니라 아래 `.asset` 파일을 선택해서 인스펙터에서 고친다.**

### 4-1. 소리 — `5.Data/Audio/AudioCatalog.asset`

모든 효과음의 클립과 수치가 여기 한 표에 모여 있다. `Entries` 목록의 한 줄이 소리 하나다.

| 항목 | 뜻 | 팁 |
|---|---|---|
| `Id` | 이 소리를 부르는 이름표 (예: `HumanNormalAttack1`) | 코드가 이 이름으로 소리를 부른다. 함부로 바꾸지 않는다 |
| `Clips` | 실제 사운드 파일. **여러 개 넣으면 그중 하나를 무작위로 재생** | 타격음처럼 반복되는 소리에 2~3개 넣으면 덜 지겹다 |
| `Volume` | 크기 (0~1) | |
| `Pitch` | 음높이. 1이 원본, 낮추면 굵어지고 높이면 가늘어진다 | |
| `Spatial Blend` | **0 = 어디서 들려도 같은 크기(2D)**, **1 = 멀어지면 작아짐(3D)** | UI·플레이어 소리는 0, 환경음은 1 |
| `Min Distance` | 이 거리 안에서는 최대 볼륨 | Spatial Blend가 0이면 무의미 |
| `Max Distance` | 이 거리 밖에서는 안 들림 | |
| `Loop` | 켜면 멈출 때까지 계속 반복 | |
| `Output` | 어느 볼륨 그룹에 속하는지 (믹서 그룹) | 설정 창의 볼륨 슬라이더가 여기로 걸린다 |

**소리를 새로 추가하려면:**
1. 사운드 파일을 `3.Content` 아래에 넣는다
2. `AudioCatalog`의 `Entries`에 줄을 하나 늘리고 클립을 꽂는다
3. `Id`를 고른다 — **쓸 수 있는 Id 목록은 정해져 있다**(4-8). 없는 소리를 추가하려면 개발자에게 Id 추가를 요청해야 한다

새 Id를 만들었는데 `Entries`에 안 넣으면, 유니티 콘솔에 `[AudioCatalog] _entries에 없는 AudioId: …` 경고가 뜬다. 이게 뜨면 빠뜨린 것이다.

볼륨 그룹은 이렇게 나뉜다: `MasterVolume`, `BgmVolume`, `SfxVolume`, `EnemyVolume`, `PlayerVolume`, `EnvironmentVolume`, `UiVolume`.

> 예외: **BGM과 적 소리는 이 표를 안 거친다.** 씬이나 적 프리팹에 붙은 AudioSource를 직접 고쳐야 한다.

### 4-2. 적 능력치 — `5.Data/Enemy/*.asset`

현재 두 개: `EnemyMinionData.asset`(잡몹), `EnemyMinotaurData.asset`(미노타우르스).

| 항목 | 뜻 | 범위 |
|---|---|---|
| `f_hp` | 체력 | 1 ~ 200 |
| `i_hpBarCount` | 체력바 칸 수 (페이즈 수) | 1 ~ 10 |
| `f_trackingSpeed` | 플레이어를 쫓을 때 이동 속도 | 1 ~ 1000 |
| `f_patrolSpeed` | 평소 순찰할 때 이동 속도 | 1 ~ 1000 |
| `i_patrolWaitingTimeMin` / `Max` | 순찰 지점에 도착해 멈춰 서 있는 시간(초). 이 사이에서 무작위 | 1 ~ 10 |
| `f_viewAngle` | 시야각(도). 클수록 넓게 본다 | 1 ~ 359 |
| `f_viewDistance` | 시야 거리 | 1 ~ 100 |
| `f_leftDeadBody` | 죽은 시체가 남아 있는 시간(초) | 1 ~ 60 |
| `f_motionSpeed` | 동작 재생 속도. 1이 원본, 높이면 빨라진다 | 0 ~ 3 |

**이 값은 같은 데이터를 쓰는 적 전부에게 적용된다.** "이 방의 이 몬스터만 세게" 같은 건 여기서 못 한다.

### 4-3. 카메라 흔들림 — `5.Data/Camera/PlayerCameraShakeData.asset`

`Shake List`의 한 줄이 "플레이어가 어떤 상태일 때 화면이 어떻게 흔들리는지"다.

| 항목 | 뜻 |
|---|---|
| `State` | 어떤 상황인지 (`NormalAttack1`, `Hit`, `FinishAttack` …) |
| `Impulse Shape` | 흔들림 파형 |
| `Amplitude Gain` | 얼마나 크게 흔들리나 |
| `Frequency Gain` | 얼마나 빠르게 떨리나 |
| `Duration` | 흔들리는 시간(초) |
| `Velocity` | 흔들리는 방향. **X·Y만 쓴다(좌우·상하).** 앞뒤로 흔들면 멀미가 나서 일부러 뺐다 |

값을 고치면 **플레이 중에도 바로 반영된다.** 플레이하면서 조정하기 좋다.

### 4-4. 플레이어 동작 — `5.Data/Player/State/*.asset`

가장 자주 만지는 곳이자 가장 복잡한 곳이다. 폴더가 셋으로 나뉜다.

| 폴더 | 언제 쓰이나 |
|---|---|
| `Common/` | 사람탈·동물탈 공통 (`LocomotionState` 이동, `FrontDashState` 앞구르기, `HitState` 피격, `DeadState` 사망) |
| `Human/` | 사람탈 전용 (`HNormalAttack1~3`, `HSpecialAttackState`, `HFinishAttackState`, `HBackDashState`) |
| `Aniaml/` | 동물탈 전용 (`ANormalAttack1~3`, `ASpecialAttackState`, `AFinishAttackState`, `ABackDashState`) |

파일 하나를 열면 맨 위에 공통 항목 세 개가 있다.

| 항목 | 뜻 |
|---|---|
| `State Type` | 이 파일이 어떤 동작인지 |
| `Is Looping` | 끝나면 반복할지 (이동은 켬, 공격은 끔) |
| `Cooldown` | 이 동작을 다시 쓰기까지 최소 대기 시간(초). **스킬 연타 방지용** |

그 아래는 **"동작 재생 중 몇 초 지점에 무슨 일이 일어나는지"** 목록이다. 각 목록은 구간(시작~끝 시간)을 여러 개 넣을 수 있다.

| 목록 | 뜻 | 쓰는 예 |
|---|---|---|
| `Input Block` | 이 구간엔 입력을 무시 | 공격 시작 직후 |
| `Input Buffer` | 이 구간에 누른 입력을 기억했다가 나중에 실행 | 연속 공격 이어치기 |
| `Move Control` | 이 구간엔 이동 가능 / 불가 | |
| `Rotate Control` | 이 구간엔 방향 전환 가능 / 불가 | |
| `Super Armor` | 이 구간엔 맞아도 안 밀림 | 큰 공격 |
| `Invincible` | 이 구간엔 아예 안 맞음 | 구르기 |
| `Camera Lock` | 이 구간엔 카메라 고정 | |
| `Skill Move` | 이 구간에 캐릭터를 앞으로 밀어냄 | 돌진 공격 |
| `Effect` | 이 구간에 이펙트 재생 | |
| `Hitbox` | 이 구간에 공격 판정 켬. **여기서 데미지가 정해진다** | |
| `Audio` | 이 구간에 소리 재생 | |
| `Camera Shake` | 이 구간에 화면 흔들림 | |
| `Camera Zoom` | 이 구간에 카메라 당김 | |
| `Finish` | 처형 판정 | |
| `Object Toggle` | 이 구간에 오브젝트를 켜고 끔 (무기 궤적 등) | |

**타이밍 조정하는 법:** 애니메이션 클립을 보면서 "칼이 지나가는 지점이 0.3초"라면 `Hitbox`의 구간을 0.3~0.45 식으로 넣는다. 값은 초 단위다.

> 인스펙터가 비어 보인다면 정상이다. 안 쓰는 항목은 에디터가 숨기게 만들어놨다.

### 4-5. 이펙트 — `5.Data/Player/PlayerEffectCatalog.asset`

| 항목 | 뜻 |
|---|---|
| `Id` | 이펙트를 부르는 이름표 |
| `Prefab` | 실제로 나올 이펙트 프리팹 |
| `Pool Size` | **미리 만들어둘 개수.** 동시에 이 개수보다 많이 나오면 렉이 생길 수 있다 |

`Pool Size` 기본값은 3이다. 연타로 여러 개가 동시에 뜨는 이펙트는 늘린다.

### 4-6. 번역 텍스트 — `5.Data/Language/TextTableData.asset`

게임 안에 나오는 모든 문장이 여기 한 표에 있다.

| 항목 | 뜻 |
|---|---|
| `Entries > Key` | 문장의 이름표 (예: `Area_Forest`) |
| `Entries > Korean` | 한국어 문장 |
| `Entries > English` | 영어 문장 |
| `Korean Font` / `English Font` | 언어별 폰트 |

**문장을 추가하려면** `Entries`에 줄을 늘리고 Key·한국어·영어를 채운다. 그 다음 UI 텍스트 오브젝트에 `LocalizedText` 컴포넌트를 붙이고 Key를 고르면 된다.

- Key를 잘못 적으면 게임에 Key가 그대로 표시되고 콘솔에 `[TextTableData] 표에 없는 키: …` 경고가 뜬다.
- 한쪽 언어를 비워두면 `[TextTableData] …의 English 문장이 비어 있음` 경고가 뜬다.
- **지역 이름은 Key를 `Area`로 시작하게 짓는다.** 세이브 포인트에서 드롭다운으로 고를 때 `Area`가 든 키만 보이게 되어 있다.

### 4-7. 그 외 데이터 파일 — 건드리지 않는 것

| 파일 | 성격 |
|---|---|
| `Audio/AudioChannel.asset` | 소리 요청을 전달하는 통로. 내용 없음 |
| `Player/Event/*.asset`, `Player/PlayerHitChannel.asset` | 시스템끼리 신호를 주고받는 통로. 내용 없음 |
| `Camera/CinemachineBlender.asset` | 카메라 전환 방식 |
| `Input/` | 조작 키 정의 |

`*Channel.asset`은 **비어 있는 게 정상**이다. 지우면 안 된다.

### 4-8. 정해진 이름표 목록 (Enum)

`Id` 칸에서 고를 수 있는 항목은 코드에 정해져 있다. **새 항목이 필요하면 개발자에게 요청해야 한다.**

**소리 Id (`SoundType`)** — 사람탈 / 동물탈 / 플레이어 공용 / UI / 파트너 그룹으로 나뉜다.
예: `HumanNormalAttack1`, `AnimalBackDash`, `FrontDash`, `Hit`, `Die`, `UIClick`, `UIHover`, `PlayerPartner`

**이펙트 Id (`EffectId`)**
예: `HumanNormalAttack1`, `AnimalSpecialAttackSlash`, `FinishAttackDome`, `HitVignette`, `SwapToAnimalMask`

**플레이어 상태 (`PlayerStateType`)**
`Locomotion`, `NormalAttack1~3`, `SpecialAttack`, `FinishAttack`, `FrontDash`, `BackDash`, `Hit`, `Dead`

**입력 액션 (`InputActionType`)**
`Movement`, `NormalAttack`, `SpecialAttack`, `FinishAttack`, `Dash`, `LockOn`, `Interaction`, `Menu`

> **중요:** 이 목록의 **순서를 바꾸거나 중간에 항목을 끼워넣으면, 이미 배치해둔 모든 값이 한 칸씩 밀린다.**
> 공격 소리가 갑자기 UI 소리로 바뀌는 식의 사고가 난다. 항목 추가는 항상 그룹 맨 뒤에다 한다.
> 자세한 규칙은 `Docs/EnumGuide.md`에 있다.

---

## 5. 기믹 배치 — 트리거와 이벤트

이 프로젝트의 연출은 **"트리거 + 이벤트 목록"** 두 조각으로 만든다.

- **트리거** = 언제 실행할지 (플레이어가 들어오면 / 씬이 시작하면 / 적을 죽이면)
- **이벤트** = 무엇을 할지 (페이드, 저장, 씬 이동, 카메라 조작 …)

### 5-1. 기본 배치 방법

1. 빈 GameObject를 만들고 원하는 위치에 놓는다
2. **트리거 컴포넌트**를 붙인다 (5-2)
3. `EnterTrigger`라면 `Box Collider`를 붙이고 **`Is Trigger`를 체크**한다. 콜라이더 크기가 곧 발동 범위다
4. 그 오브젝트(또는 자식)에 **이벤트 컴포넌트**를 붙인다 (5-3)
5. 트리거의 이벤트 목록에 붙인 이벤트들을 드래그해 넣는다
6. `Delay Times`에 각 이벤트를 몇 초 뒤에 실행할지 적는다. **비워두면 0초(즉시)**

이벤트 목록과 `Delay Times`는 **같은 순번끼리 짝**이다. 첫 번째 이벤트는 첫 번째 지연 시간을 쓴다.

### 5-2. 트리거 종류

| 컴포넌트 | 언제 실행되나 | 주의 |
|---|---|---|
| `EnterTrigger` | 플레이어가 범위에 들어오면 | 여러 번 발동시키려면 **`Is Loop`를 켠다** |
| `StartTrigger` | 씬이 시작할 때 | `Run On` 설정 필수 (아래) |
| `InteractionTrigger` | 범위 안에서 상호작용 키를 누르면 | 문 열기 등. 타임라인을 재생한다 |
| `Enemy`의 `Death Event` | 그 적을 죽이면 | 적 프리팹 인스펙터의 "죽였을 때 실행할 이벤트" |
| `CutSceneSkipTrigger` | 컷씬 중 ESC / 스킵 버튼 | 스킵 가능 구간은 타임라인 Signal로 지정 |

**`StartTrigger`의 `Run On`** — 씬에 어떻게 들어왔는지에 따라 실행 여부가 갈린다.

| 값 | 실행되는 경우 |
|---|---|
| `Always` | 어떻게 들어왔든 항상 |
| `OnAreaEnter` | 앞 지역에서 넘어왔거나 새 게임으로 처음 들어온 경우 |
| `OnSlotLoad` | 세이브를 불러왔거나 낙사로 다시 시작한 경우 |

**시작 연출(카메라 워크 등)은 반드시 `OnAreaEnter`로 둔다.** `Always`로 두면 세이브를 불러올 때마다 연출이 다시 돌면서 저장된 위치를 덮어쓴다.

### 5-3. 이벤트 종류 전체

#### 화면·연출

| 컴포넌트 | 하는 일 | 주요 항목 |
|---|---|---|
| `EventUIFade` | 화면을 어둡게/밝게 | `Fade In`(켜면 어두워짐), `Fade In Duration`, `Hold Duration`(0이면 그대로 유지), `Fade Out Duration` |
| `EventButtonGuide` | 버튼을 반짝이며 안내 | `Target Button`, `Objects To Enable`(같이 켤 것), `Objects To Disable`(끌 것), `Blink Min Alpha`(0이면 완전히 사라짐), `Blink Speed`(클수록 빠름) |
| `EventFinishGuide` | 처형 가능한 적이 처음 나오면 자동으로 안내 | `Guide`에 `EventButtonGuide`를 꽂는다. **1회만 뜬다** |
| `EventOutlineOn` / `EventOutlineOff` | 오브젝트에 외곽선을 켜고 끔 | `Renderer`, `Outline Material` |
| `EventLightIntensityZero` | 조명을 끔 | `Light`. **조명을 아예 끄지 않고 밝기만 0으로 만든다** — 완전히 끄면 렉이 생기기 때문 |

#### 카메라

| 컴포넌트 | 하는 일 | 주요 항목 |
|---|---|---|
| `EventStartView` | 카메라가 보는 각도를 맞춤 | `Horizontal`(좌우 각도), `Vertical`(상하 각도) |
| `EventBossCamera` | 보스전용 카메라 거리·높이로 전환 | `Boss Top`/`Center`/`Bottom`, `Boss Follow Offset`, `Boss Target Offset` |
| `EventCameraStop` | 카메라가 대상 따라가기를 멈춤 | `Kind` (`Default` 또는 `LockOn`) |
| `EventCameraCart` | 스플라인 위 궤적 인디케이터를 이동 | `Cart`, `Trail` |
| `EventPreloadRender` | 씬 시작 시 오브젝트를 미리 한 번 그려 첫 등장 렉을 없앰 | `Preview Camera`, `Light`, `Hold Seconds`(기본 0.2) |

#### 플레이어·진행

| 컴포넌트 | 하는 일 | 주요 항목 |
|---|---|---|
| `EventCutsceneLock` | 조작을 막거나 푼다 | 체크하면 막기, 해제하면 풀기. **막는 곳과 푸는 곳에 각각 하나씩 놓는다** |
| `EventPlayerWarp` | 플레이어를 정해둔 자리로 이동 | `Player Transform`(목표 위치가 될 빈 오브젝트) |
| `EventDamage` | 플레이어에게 피해 | `Damage` (기본 3) |
| `EventFallRespawn` | 낙사 복귀 | 붙이는 `EnterTrigger`의 **`Is Loop`를 꼭 켠다.** `EventDamage`와 같은 트리거에 넣고 데미지 → 복귀 순서로 배치 |
| `EventSaveGame` | 세이브 포인트 | `Save Point Key`(지역 이름 번역 키, 드롭다운), `Object States`(같이 기억할 오브젝트 켬/끔), 시작 시 저장 체크 |
| `EventSceneLoad` | 다른 씬으로 이동 | 씬 번호, 새 게임이면 저장 삭제 체크, 대기 시간 |
| `EventParentActive` | 부모 오브젝트를 켜고 끔 | |
| `EventObjectMove` | 오브젝트를 움직임 | |

#### 소리

| 컴포넌트 | 하는 일 |
|---|---|
| `EventBackgroundSoundStart` | 배경음 시작 |
| `EventBackgroundSoundEnd` | 배경음 종료 |

#### 그 외 컴포넌트

| 컴포넌트 | 하는 일 | 주요 항목 |
|---|---|---|
| `FloatingObject` | 오브젝트를 위아래로 둥실둥실 | 왕복 방향과 진폭 (`(0, 0.5, 0)`이면 Y축 ±0.5), 왕복 속도. **트리거로 켜고 끈다(호출할 때마다 반전)** |
| `OutlineHighlight` | 강조가 필요한 오브젝트에 붙임 | `Outline Material` |
| `PreloadTarget` | 미리 그려둘 배경/몬스터에 붙이는 표식 | **붙이기만 하면 된다.** 별도 등록 불필요 |
| `ScriptedWalker` | 컷씬 중 캐릭터를 목표 지점까지 걷게 함 | 목표 지점, 도착 판정 거리, 걷기 애니 이름. **타임라인 Signal에서 `StartWalk`/`StopWalk` 호출** |

### 5-4. 자주 쓰는 조합

**씬 이동 문 만들기**

```
빈 오브젝트 + EnterTrigger (Is Trigger 켠 Box Collider)
 ├ EventUIFade      (Fade In = 켬, Duration 1초)   → Delay 0
 └ EventSceneLoad   (씬 번호, 대기 시간 1초)        → Delay 0
```

`EventSceneLoad`의 **대기 시간을 `EventUIFade`의 시간과 손으로 맞춰야** 화면이 다 어두워진 뒤에 씬이 넘어간다.

**낙사 구역 만들기**

```
바닥 아래 넓은 EnterTrigger (Is Loop 켬)
 ├ EventDamage       (Damage 3)   → Delay 0
 └ EventFallRespawn                → Delay 0.5
```

**세이브 포인트 만들기**

```
빈 오브젝트 + EnterTrigger
 └ EventSaveGame  (Save Point Key = Area_XXX)
```

저장 위치는 **플레이어 좌표가 아니라 이 오브젝트의 위치**다. 벽에 낀 곳에 놓지 않도록 주의한다.

**씬 시작 연출**

```
빈 오브젝트 + StartTrigger (Run On = OnAreaEnter)
 ├ EventPlayerWarp   → Delay 0
 ├ EventStartView    → Delay 0
 └ EventUIFade (Fade In 끔 = 밝아짐) → Delay 0.2
```

---

## 6. 적 배치하기

1. `2.Level/LevelPrefabs/3.GamePlay/Enemy/`에서 원하는 적 프리팹을 씬에 드래그
2. 인스펙터에서 아래를 채운다

| 항목 | 설명 |
|---|---|
| `Enemy Data Melee` | 능력치 에셋 (`5.Data/Enemy/…`). **비워두면 능력치가 0이 된다** |
| `Patrol Point A` / `B` | 순찰 지점 두 곳. 빈 오브젝트를 두 개 만들어 꽂는다. **안 꽂으면 제자리에 서 있는다** |
| `Is Kill Trigger` | 이 적을 죽이는 게 진행 조건이면 켠다 |
| `Kill Trigger` | 위를 켰다면 대응하는 트리거를 꽂는다 |
| `Death Event` | 죽었을 때 실행할 이벤트 목록 |
| `Delay Times` | 각 이벤트의 지연 시간(초) |
| `Attack Root` | 공격 판정이 나가는 기준점 |
| `View Transform` | 시야 판정 기준점 (눈 위치) |
| `Audio Source Idle/Attack/FindTarget/Hit/Die` | 상황별 소리. 프리팹에 이미 자식으로 들어 있다 |
| `Knockback Force` / `Duration` / `Pull Distance` | 맞았을 때 밀려나는 정도. **이건 개체별로 다르게 줄 수 있다** |

체력·속도·시야는 `EnemyData` 에셋에서 오므로 **여기서 개별로 못 바꾼다.** 특정 적만 다르게 하려면 새 `EnemyData` 에셋을 만들어야 한다.

---

## 7. 자주 겪는 문제

| 증상 | 원인 | 해결 |
|---|---|---|
| 씬을 열었는데 아무것도 안 움직임 | `Static` 프리팹이 없음 | `1.Systems/Static`을 씬에 넣는다 |
| 소리가 안 남 | `AudioCatalog`에 그 Id 줄이 없음 | 콘솔의 `_entries에 없는 AudioId` 경고 확인 |
| 소리가 멀어져도 안 작아짐 | `Spatial Blend`가 0 | 1로 올린다 |
| UI 텍스트에 Key가 그대로 보임 | 번역 표에 그 Key가 없음 | `TextTableData`에 줄 추가 |
| 트리거를 밟아도 아무 일이 없음 | Collider의 `Is Trigger`가 꺼졌거나 이벤트 목록이 비었음 | 둘 다 확인 |
| 낙사 구역이 한 번만 작동함 | `EnterTrigger`의 `Is Loop`가 꺼짐 | 켠다 |
| 세이브 불러오면 시작 연출이 또 돎 | `StartTrigger`의 `Run On`이 `Always` | `OnAreaEnter`로 바꾼다 |
| 새 `.asset`을 만들었는데 게임이 무시함 | `SOContainer`에 등록 안 함 | `Static > SOContainer`의 그룹에 넣는다 |
| 공격 소리가 갑자기 다른 소리로 바뀜 | Enum 항목이 중간에 끼워넣어져 번호가 밀림 | 개발자에게 알린다. `Docs/EnumGuide.md` 참고 |
| 이펙트가 연타할 때 안 나옴 | `Pool Size`가 부족 | `PlayerEffectCatalog`에서 늘린다 |
| 적이 제자리에만 있음 | `Patrol Point A/B` 미지정 | 빈 오브젝트 두 개를 만들어 꽂는다 |
| 씬을 추가했더니 기존 세이브가 이상한 씬을 엶 | 빌드 씬 번호가 밀림 | 새 씬은 목록 맨 뒤에 붙인다 |

---

## 8. 테스트 중 저장 데이터 지우기

세이브 파일은 유니티가 정한 개인 폴더(`persistentDataPath`) 아래 `SaveData` 폴더에 `.json`으로 쌓인다.
윈도우에서는 보통 `C:\Users\<사용자>\AppData\LocalLow\<회사명>\<게임명>\SaveData\` 다.
초기 상태로 테스트하려면 이 폴더를 통째로 지우면 된다.

---

## 9. 건드리면 안 되는 것

| 대상 | 이유 |
|---|---|
| `Assets/1.Code/` 안의 모든 파일 | 코드다 |
| Enum 항목의 순서 | 배치해둔 값이 전부 밀린다 |
| `*Channel.asset` 파일 | 시스템 배선용이다. 비어 있는 게 정상 |
| `Static` 프리팹 안의 구조 | 시스템 배선이 끊긴다 |
| 빌드 설정 씬 번호 (중간 삽입) | 세이브 데이터가 깨진다 |
| `Assets/Settings/`, `Plugins/` | 유니티/외부 설정 |
