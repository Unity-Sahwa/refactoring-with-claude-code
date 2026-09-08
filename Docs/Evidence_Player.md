# 리팩토링 근거 — 플레이어 시스템 (레거시 대비 실측)

**주제: 코드에 박혀 있던 상태를 데이터로 옮기기.** 상태별 클래스·코루틴 복제를 SO와 진행률 기반 이벤트로 교체함.

`PortfolioDraft.md`에서 분리한 문서. 여기 적힌 것은 양쪽 코드 실측이거나 본인 답변임. 추측은 적지 않음.

비교 대상
- before: `Unitysahwa-refactoring-with-claudecode(before)/Assets/Scripts/Player` (36파일)
- after: `Unitysahwa-refactoring/Assets/1.Code/Scripts/PlayerSystem` (81파일)
- 범위: 플레이어 한정. 다른 시스템은 미조사.

## 총량 지표 (실측)
| 지표 | before | after |
|---|---|---|
| 총 줄 수 | 8,854 | 4,387 |
| 최대 단일 파일 | `AnimalMaskSkill` 1,154줄 | `PlayerPartner` 212줄 |
| 싱글톤 `.Instance` 호출 | 47 | 0 (남은 4건은 풀링 객체 참조 `active.Instance`, 싱글톤 아님) |
| `GetComponent` 계열 | 59 | 10 |
| `interface` 정의 | 0 | 26 |
| `StartCoroutine` | 87 | 4 |
| 캐릭터 타입을 직접 아는 파일 | 25 (분기 150건 이상) | 4 (각 1건) |

주의: after는 스킬 수치가 SO로 빠져 코드 줄에 잡히지 않음. "코드량만의 비교"임을 명시할 것.
주의: float 리터럴 총량은 118 → 94로 대비가 약함. 지표로 쓰지 말 것.

## 중복 실측
- `CoFirstAttack`: Animal·Human 양쪽 모두 **111줄로 길이 동일**, 약 70% 동일
- `CoDash`: 128줄 / 121줄, 약 84% 동일
- 동일 이름·동일 역할 메서드 12개가 두 클래스에 중복

## 기능별 구조 대비

### 콤보 공격
| 축 | before `CoFirstAttack` | after `StateRunner` |
|---|---|---|
| 규모 | 111줄 × 6벌 (3콤보 × 2캐릭터) | 100줄 1개가 상태 16종 처리 |
| 직접 참조 | 컴포넌트 9개 | 인터페이스 1개 (`IPlayerStateEventRaiser`) |
| 1회 실행 보장 | `activeXxxOnce` 불리언 6개 수동 | 정렬 이벤트 목록 + 커서 `_readIndex` 1개 |
| 타이밍 기준 | 절대 초 `Time.time + waitTime` | 애니 진행률 0~1 |
| 중단 처리 | 애니 해시 분기 + `yield break` 산재 | `Exit()` → `RaiseReset()` 1곳 |

핵심: 줄 수가 아니라 책임 위치. before는 "언제·무엇을·어떻게"를 한 메서드가 전부 앎.
after는 언제=진행률(`StateRunner`), 무엇=`StateData` SO, 어떻게=구독자로 분리.

### 이동
| 축 | before | after |
|---|---|---|
| 이동 원인 실행 | 각 스킬 코루틴이 `playerSkillMove.StartCoroutine(SkillMove(...))` 직접 호출 | `IVelocitySource.Evaluate`가 속도만 반환 (Walk/Skill/Gravity 3종) |
| 합산 지점 | 없음 (원인끼리 서로 모름) | `PlayerCharacterMover` 1곳 |
| 방향 확장 | bool 4개 + Vector3 4개 필드 세트 추가 | 구현 클래스 1개 추가, Mover 무변경 |
| 의존 방향 | `PlayerSkillMove`가 `HumanMaskSkill`을 `SerializeField` 역참조 | Mover → 인터페이스만 |

### 히트박스
| 축 | before | after |
|---|---|---|
| 히트박스 실체 | 씬에 배치한 GameObject 배열, 수동 배선 | 없음. `Physics.Overlap*NonAlloc`으로 그 순간만 판정 |
| 대상 선택 | `SelectHitBox`의 하드코딩 switch 6 case | `HitboxDataEntry`가 위치·회전·형태를 SO에 기술 |
| 판정 코드 | `OnCollisionEnter` 단일 메서드 244줄 (41~284행, 사이에 다른 메서드 없음 확인) | `TryHit` 39줄 + `IDamageable` 계약 |
| 직접 참조 | `PlayerHitBoxCollider`가 컴포넌트 7개 `SerializeField` | 주입 3개, 전부 인터페이스/채널 |
| 중복 타격 방지 | 없음 (`HashSet`/`List`/`Contains`/카운트 0건 확인. 파일에 "타격 횟수 추가해야함" 미구현 주석 존재) | `HashSet<IDamageable> AlreadyHit` |

콤보 1타 추가 비용: before는 씬 오브젝트 + 배선 + switch case + enum(3파일 이상) / after는 SO 배열 항목 1개.

### 피격 반응
- before: `OnCollisionEnter` 244줄이 `switch (playerCurrentSubState)`로 공격 종류를 분기하고,
  각 case마다 이펙트·타임스케일·카메라쉐이크·사운드 4줄을 통째로 복제. 확인된 것만 Human 4종 + Animal 4종 = 8벌
- before: 값이 `humanData.firstNormalAttackHitEffect` 식으로 공격별 필드 4개씩 존재
  (`PlayerHumanMaskData` 102필드 / `PlayerAnimalMaskData` 103필드 / `PlayerGhostMaskData` 42 / `PlayerCommonData` 47의 정체)
- after: 히트박스 데이터가 `CombatInfo`를 실어 보내고 `HitEffectHandler`·`HitStopHandler`·`HitFlashHandler`·`HitVignetteEffect`가 각자 구독. 분기 0, 복제 0
- 스킬 1개 추가 비용: before = enum + Data 필드 4개 + case 1개 + 반응 4줄 복제 / after = SO 항목 1개

### 캐릭터 스왑
| 축 | before `MaskChange` (269줄) | after `PlayerCharacterSwitcher` (74줄) |
|---|---|---|
| 참조 컴포넌트 | `SerializeField` 9개 + `CameraController.instance` 직접 호출 | 주입 1개 (`List<PlayerCharacter>`) |
| 캐릭터별 필드 | Human/Animal 각각 GameObject·Animator·Rigidbody 3세트 9필드 + Current 3필드 | 없음. `PlayerCharacter.GetCharacterComponent<T>()`로 조회 |
| 전환 시 하는 일 | 위치·회전 복사, current 3개 갱신, `humanSkill.InitializeSkill()` 직접 호출, `SetActive`, 카메라 락온 조회해 `isFocused` 세팅, `skillHUD.ChangeIcon`, 마스크 오브젝트 4개 토글 | 위치·회전 복사, `SetActive`, `OnCharacterSwapped` 발행 |
| 전환 후속 처리 | 스왑 클래스가 직접 호출 | 구독자가 각자 처리. 구독처 23곳 |
| 결합 방향 | 다른 21개 파일이 `MaskChange.instance` / `maskChange.` 로 역참조 | 인터페이스 3종만 노출 (`ICharacterSwappable`, `ICharacterSwapNotifier`, `ICurrentCharacterProvider`) |

핵심: before는 스왑 클래스가 후속 처리 전부를 알아야 함. after는 "바꿨다"만 알림.

### 이펙트 수명
- before: `Instantiate` 0건. 씬에 미리 배치한 `GameObject[]` 슬롯 16개를 인스펙터에 수동 배선하고 코루틴이 `SetActive` 토글. 풀 고갈 개념 없음
- before `Destroy` 6건은 전부 `else if (instance != this) Destroy(gameObject)` — 싱글톤 중복 제거용. Player 폴더에만 싱글톤 클래스 6개
- after: `EffectCatalog`(SO)에 id·프리팹·풀 크기 등록 → `PlayerEffectProvider`가 풀 자동 생성, `Rent`/`Return`. 프리로드 대상도 자동 제공. 풀 고갈 시 경고
- 차이의 본질: 양쪽 다 미리 만듦. **관리 주체가 사람(인스펙터) → 코드(카탈로그)로 이동**



## 결합 구조 실측

### 순환 참조 — 양방향 `SerializeField` 6쌍 확정
| A | B |
|---|---|
| `HumanMaskSkill` | `PlayerSkillMove` |
| `HumanMaskSkill` | `PlayerAnimation` |
| `HumanMaskSkill` | `PlayerSkillInput` |
| `AnimalMaskSkill` | `PlayerAnimation` |
| `AnimalMaskSkill` | `PlayerSkillInput` |
| `PlayerMovement` | `PlayerState` |

- 6쌍 모두 양쪽 파일에서 상대 타입을 `SerializeField`로 들고 있음을 확인
- 추가로 `PlayerController`는 `public static instance`를 두고 3곳이 역참조 → 컴파일 의존은 단방향이나 실질 순환
- after는 순환 없음. 의존이 인터페이스·채널을 통해 한 방향으로만 흐름

### MonoBehaviour 비중
| | before | after |
|---|---|---|
| 전체 파일 | 36 | 81 |
| MonoBehaviour | 25 (69%) | 23 (28%) |
| 비-MonoBehaviour | 11 | 58 (그중 interface 26) |

의미: before는 대부분이 씬에 붙어야만 존재하는 컴포넌트라 씬 없이는 읽기도 검증도 어려움.
after는 절반 이상이 순수 C# 클래스·인터페이스라 코드만으로 계약을 파악할 수 있음.

### enum 관리
- before: `PlayerDataType.cs` 단일 파일 467줄에 enum 11종
  (`SkillCooldown`·`MaskType`·`FunctionTarget`·`HitBoxType`·`CameraShakeType`·`CameraReactionType`·`TimeScaleApplyTarget`·`PlayerStateType`·`PlayerSubStateType`·`PlayerRestrictionType`·`MoveDirection`)
- before는 상태를 `PlayerStateType` + `PlayerSubStateType` 2단으로 관리 (예: NORMALATTACK / FIRSTNORMALATTACK)
- after: 파일당 enum 1개로 분리. `PlayerStateType`은 20줄. 서브 상태 개념 없이 상태 하나로 평탄화

## 기능 대조표 (레거시 36파일 → 현재)
목적: 리팩토링으로 기능이 사라진 게 아니라 자리를 옮겼음을 보이는 것.

| 레거시 (Assets/Scripts/Player) | 줄 | 현재 대응 | 비고 |
|---|---|---|---|
| `HumanMaskSkill` | 1,127 | `States/StateRunner` + `StateData` SO | 캐릭터별 복제 → 코드 1 + 데이터 N |
| `AnimalMaskSkill` | 1,154 | 동상 | |
| `GhostMaskSkill` | 583 | `Finish/FinishExecutor` + `FinishTargetScanner` + `IFinishChecker` | 처형 기능 |
| `PlayerMovement` | 404 | `Move/PlayerCharacterMover` + `WalkVelocitySource` + `GravityVelocitySource` + `GroundProbe` + `CharacterRotator` | Rigidbody → CharacterController |
| `PlayerEffect` | 366 | `Effect/PlayerEffectHandler` + `PlayerEffectProvider` + `EffectCatalog` + `HitVignetteEffect` | 수동 배선 → 풀 |
| `Player` | 340 | `Damage/PlayerHealth` + `PlayerDeathTrigger` + `PlayerDamageReceiver` | 싱글톤 해체 |
| `PlayerHitBoxCollider` | 292 | `Attack/PlayerHitboxHandler` + `HitChannel` + `CombatSystem/IDamageable` | 244줄 단일 메서드 해체 |
| `PlayerController` | 281 | 여러 시스템으로 분해 (입력은 InputSystem으로 이동) | |
| `PlayerMaskChange`(MaskChange) | 269 | `Character/PlayerCharacterSwitcher` + 인터페이스 3종 | |
| `PlayerAnimation` | 267 | `States/AnimationTracker` + `Move/PlayerMoveAnimation` | |
| `PlayerSkillMove` | 244 | `Move/SkillVelocitySource` + `SkillMoveDataEntry` | |
| `PlayerSkillInput` | 223 | `States/PlayerStateInputGate` + `IntervalDataEntry`(InputBlock·InputBuffer) | |
| `PlayerSound` | 209 | `Audio/PlayerAudioHandler` + `AudioDataEntry` + `FootstepEmitter` | |
| `PlayerState` | 165 | `States/PlayerStateMachine` + `PlayerStateType` + `CurrentState` 채널 | |
| `PlayerCameraEffect` | 127 | `CameraSystem/PlayerCameraShake` + `PlayerCameraZoomHandler` | 플레이어 밖으로 이동 |
| `PlayerHitBox` | 103 | `Attack/PlayerHitboxHandler` + `HitboxDataEntry` | 씬 오브젝트 → Overlap |
| `GameTimeScale` | 75 | `CombatSystem/HitStopHandler` | |
| `PlayerSensor` | 59 | `CharacterController`가 대체 | `PlayerMovement`·`PlayerSkillMove`가 쓰던 전·후방 벽 감지 Update. 기능 제거가 아니라 엔진 기능으로 대체 |
| `PlayerTeleporter` | 29 | `GimmickSystem/EventPlayerWarp` | |
| `PlayerWaypoints` | 32 | 기믹 시스템으로 대체 | 본인 답변 |
| `PlayerDamageReaction` | 23 | `Damage/PlayerDeathTrigger` | |
| `TimelineHelper` | 280 | 전부 분리함 | 싱글톤 + `SerializeField` 7개(MaskChange·PlayerState·PlayerSound·MenuUI·CameraController·UIEffect·MouseSettingUI), 외부 4곳이 역참조. 컷씬을 빌미로 플레이어·UI·카메라·씬을 한 클래스가 다 알던 구조 |
| `MobileInput` | 96 | InputSystem으로 이동 | 플레이어 밖 |
| `PlayerSkill` | 93 | `StateData` SO | |
| `PlayerHumanMaskData` | 375 | `StateData` SO 16개 | 102필드 단일 클래스 해체 |
| `PlayerAnimalMaskData` | 329 | 동상 | 103필드 |
| `PlayerGhostMaskData` | 164 | 동상 | 42필드 |
| `PlayerCommonData` | 187 | 동상 + `PlayerCharacterMover` 인스펙터 값 | 47필드 |
| `PlayerDataType` | 467 | `PlayerStateType`·`StateTriggerType`·`StateEventCategory` 등으로 분리 | enum 단일 파일 해체 |
| `PartnerData` | 74 | `Partner/PlayerPartner` | |
| `UIEffectData` | 62 | UISystem으로 이동 | 플레이어 밖 |
| `CheatData` | 124 | 제외 | 에디터 전용. 본인 답변 |
| `SetPlayerData` | 32 | 대응 없음 | 데이터 구조체 정의용. `CameraData`가 참조 |
| `EffectTimer` | 128 | 없음 — **파일 전체가 주석 처리된 죽은 코드** | |
| `ObjectLinker` | 20 | 없음 — **메서드 본문이 전부 빈 껍데기** | |
| `Visualize` | 51 | 없음 — `Debug.Log` 테스트용 | |

죽은 코드: 레거시 36파일 중 3개(`EffectTimer`·`ObjectLinker`·`Visualize`)가 실행되지 않는 코드였음.
앞서 미확정이던 3건(`PlayerSensor`·`TimelineHelper`·`SetPlayerData`)은 모두 실사용 코드로 확인됨. 죽은 코드 아님.

## 서술 시 주의 (감사 지적 사항)
- `PlayerCharacterSwitcher.SwapPlayerCharacter`는 "타입이 다른 첫 번째"를 고르는 2종 전제 구현.
  기획상 캐릭터 2종이 확정 사항이므로 결함이 아니라 범위 결정. "코드 0줄"이 아니라 "확정된 2종 범위 안에서 데이터만으로 대응"으로 쓸 것
- 초안에 적힌 "상태 클래스 상속 구조를 폐기"는 레거시 실물과 다름. 실제 before는 상속이 아니라
  **캐릭터별 거대 스킬 클래스 복제**(`AnimalMaskSkill`/`HumanMaskSkill`/`GhostMaskSkill`). 서술 수정 필요
- 초안의 "대쉬 중 공격 시 대쉬 이동이 스킬에 적용되던 버그"는 리팩토링 이후 코드에서 난 것.
  레거시 비교 근거로 연결하지 말 것
- `_Refactoring/Prototype/Player/States`(BaseState + 파생 8개)는 테스트용이며 before가 아님

---

# [미평가 — 평가 후 삭제할 것]

## 1. 기능 동등성 증명
구조를 바꿨는데 같은 게임이 돌아간다는 증거가 필요함. 없으면 "코드를 줄인 게 아니라 기능이 빠진 것"으로 읽힘.

확인된 사실 (본인 답변)
- 레거시 프로젝트는 실행 가능한 상태로 남아 있음
- 기능은 동일. 차이는 연출과 공격 사이 히트박스 몇 부분이 추가된 정도
- `CheatMode`: 에디터 전용이라 제외
- `TimelineHelper`: 결합 덩어리라 전부 분리함
- `PlayerWaypoints`: 기믹 시스템으로 대체 (SavePoint 계열 클래스로 추정 — 확인 필요)

해야 할 것
- [x] 기능 대조표 작성 완료 (아래 "기능 대조표" 절). 미확정 3건 남음
- [ ] 녹화 3종 확보: 에디터 플레이 / APK 실기기 / AAB(Play 배포본) 실기기. 각각 같은 구간
- [ ] 레거시 쪽도 동일 구간 녹화. 캡션에 측정 조건(에디터인지 실기기인지) 명시
- [ ] 추가된 히트박스·연출 부분은 "동등"이 아니라 "추가"로 따로 표기

## 2. 왜 그 설계였나 (본인 답변)

### 상태 전환만 코드에 남긴 이유
- 상태와 상태 전환은 다른 문제로 봄. 전환에는 콤보·스왑 같은 예외 규칙이 있어 표(데이터)로 표현되지 않음
- 그래서 `TryGetNextState`의 `switch`에 하드코딩. 전환 규칙을 한눈에 보는 것도 목적
- 코드 주석에 이 이유를 명시함 (`PlayerStateMachine.cs`)
- `(_, Attack) => NormalAttack1` 와일드카드는 의도. 콤보는 1타로 초기화되어야 함

### 카테고리 15종은 설계와 증축이 섞임
- 대부분 먼저 설계했으나 만들다가 추가한 것이 3~4개 정도 됨
- AI로 빠르게 추가할 수 있어 쉬웠던 것이 오히려 무분별한 증축을 부름
- 지금 구조의 단점: 한 상태에만 필요한 기능도 `StateData`에 필드가 생겨 나머지 15개 SO에 전부 노출됨.
  `Finish`(처형)가 그 예. 다른 상태는 값을 안 넣으면 되지만, 클래스 단위로 분리하지 못한 점은 아쉬움
- 완화된 부분: `IntervalDataEntry`가 7개 카테고리(InputBlock·InputBuffer·MoveControl·RotateControl·SuperArmor·Invincible·CameraLock) 공용이라
  "구간 on/off"류는 enum 값 추가만으로 늘어남. 전용 카테고리만 문제로 남음

### 데이터화의 협업 근거
- 수치를 데이터로 뺀 이유는 협업. 오브젝트를 하나하나 찾아 수치를 박는 방식은 비개발자가 손댈 수 없음
- 현재 인스펙터로 값을 조정한 사람은 본인뿐. 기획자 투입 예정이나 아직 실증 없음 → 협업 효과는 근거로 쓰지 말 것

## 3. 성능 영향 — 비교 불가로 확정 (측정하지 않음)
레거시는 Unity 2022 LTS, 현재는 Unity 6. 메이저 2단계 차이라 URP 개편·GC·컴파일러가 모두 다름.
어떤 수치가 나와도 코드 기여분과 엔진 기여분을 분리할 수 없음. 실기기 측정은 발열·백그라운드 편차까지 더해짐.
after 단독 수치는 비교 대상이 없어 리팩토링 성과와 연결되지 않으므로 쓰지 않음.

"구조를 늘렸는데 느려지지 않았냐"는 질문에는 아래 정적 사실로 답함.
- before는 `StartCoroutine` 87건. 각 스킬 코루틴이 `while` + `yield return null`로 매 프레임 돌며
  `activeXxxOnce` 플래그 6개를 계속 검사함. `Update` 수(3개)가 적은 게 프레임당 작업이 적다는 뜻이 아님
- after는 `StartCoroutine` 4건. `StateRunner`가 정렬된 이벤트 목록을 커서 `_readIndex`로 진행률 도달분만 발행
- 정적 지표 대비(참고): `Update`/`FixedUpdate`/`LateUpdate` 3/1/0 → 7/1/1, `foreach` 3 → 16, `new Dictionary` 0 → 4

남은 개선 여지 (스스로 확인한 것)
- `StateRunner.Enter`의 `StateKey.ToString()` 2건이 상태 진입마다 문자열 할당. 해시 캐시로 제거 가능

## 4. 그 외 미확인
- 리팩토링에 든 기간·커밋 수를 플레이어 시스템 한정으로 분리 가능한지
- 리팩토링으로 새로 생긴 문제(회귀 버그)가 있었는지, 있었다면 어떻게 잡았는지
- 플레이어 외 시스템(카메라·입력·오디오·UI·저장) 레거시 대비 미조사

## 5. 다음에 할 것
- 여러 데이터 그룹의 수치를 한 곳에 모아 표 형태로 편집하는 SO 에셋 (엑셀 파일이 아니라 엑셀 느낌의 에디터).
  비개발자가 그룹이 달라도 한 화면에서 값을 넣게 하는 것이 목적. `TextTableCsv`의 CSV 임포트 방식을 수치로 확장하는 경로가 있음
- `StateData`의 상태 전용 카테고리(`Finish` 등)를 분리해 나머지 SO에 노출되지 않게 하는 방법 검토
