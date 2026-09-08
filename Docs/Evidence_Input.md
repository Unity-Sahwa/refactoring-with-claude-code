# 리팩토링 근거 — 입력 · 게임 상태 (레거시 대비 실측)

**주제: "지금 입력을 받아도 되는가"를 매번 되묻지 않기.** 조건 검사로 판별하던 것을 게임 상태와 도메인 핸들러로 나눔.

여기 적힌 것은 양쪽 코드 실측임. 추측은 적지 않음.

## 총량 지표 (실측)
| 지표 | before | after |
|---|---|---|
| 입력 전담 시스템 | 없음 | `InputSystem` 12파일 396줄 |
| 게임 상태 전담 시스템 | 없음 | `GameStateSystem` 6파일 168줄 |
| `Input.Get*` 직접 호출 | **40회 / 7개 파일** | 0 (Unity Input System `InputActionAsset` 사용) |
| 입력 계약 인터페이스 | 0 | 6 |

## before: 입력이 필요한 곳마다 직접 물어봄
`Input.Get*` 호출 위치 — `CheatMode` 15 · `PlayerController` 14 · `PlayerMovement` 6 ·
`TextManager` · `InteractionTrigger` · `InputKeySettingUI` · `EventTypingEffect` 각 1

`PlayerController.Update()` 하나의 구조
- 매 프레임 폴링. `Input.GetKeyDown(saveManager.InputKeys[KeyAction.X])`를 조건마다 반복 호출
- 입력을 받아도 되는지를 **가드 조건 나열로 판별**:
  `menuUI.isPlayerControlDisabled` / `playerState.playerCurrentState == DEAD` / `playerState.doNotAct` /
  `player.IsPerformingHitAction` / `playerState.isPerfomingSklill` / `!PlatformSwitcher.instance.IsPCPlatform`
- 캐릭터 분기(`maskChange.CurrentMask == maskChange.HumanMask`)가 입력 처리 안에 들어가
  같은 키 처리가 캐릭터마다 복제됨
- 키를 눌렀을 때 할 일을 `humanMaskSkill.NormalAttack()` 식으로 **구체 클래스를 직접 호출**

게임 상태 관리
- 전담 클래스 없음. `Time.timeScale` 직접 조작 9곳,
  `isPaused` / `IsPlayingMainMenu` / `IsTimelinePlaying` 같은 개별 bool 조회 13곳으로 대신함
- "지금 게임 중인가 / 메뉴인가 / 컷씬인가"가 한 곳에 없고 각자 판단함

## after: 상태가 도메인을 고르고, 핸들러가 처리
| 구성 | 줄 | 책임 |
|---|---|---|
| `InputHub` | 166 | 입력을 받아 현재 게임 상태에 맞는 도메인 핸들러로 분배 |
| `GameplayInputHandler` | 18 | 게임플레이 입력 처리 |
| `MenuInputHandler` | 17 | 메뉴 입력 처리 |
| `CutsceneInputHandler` | 27 | 컷씬 입력 처리 |
| `IDomainInputHandler` | 14 | 도메인 처리 계약 |
| `MobileInputButton` | 65 | 모바일 버튼을 같은 경로로 흡수 |
| `InputKeySettings` | 30 | 키 리바인딩 저장 연동 |

- `InputHub`가 `Dictionary<GameStateType, IDomainInputHandler>`를 만들고,
  입력이 오면 `_gameStateProvider.Current`로 핸들러를 **찾아서 넘김**. 가드 조건 나열이 사라짐
- 소비자는 `IInputPressedProvider`·`IInputMoveProvider`·`IMenuInputProvider`·`ICutsceneInputProvider` 중
  필요한 것만 주입받음. 입력 원본(PC/모바일)을 모름
- `GameStateManager`(56줄)가 상태를 `Push`/`Pop`으로 쌓아 관리하고,
  `CursorController`·`GamePauseController`가 상태 변화를 받아 각자 처리

## 핵심 대비
| | before | after |
|---|---|---|
| 입력 방식 | 매 프레임 폴링 | 이벤트 |
| 입력 가능 여부 | 가드 조건 6종을 나열해 판별 | 현재 게임 상태로 핸들러를 선택 |
| 캐릭터 분기 | 입력 처리 안에서 분기, 키 처리 복제 | 없음 (상태머신이 담당) |
| 실행 대상 | 구체 클래스 직접 호출 | 이벤트 발행, 구독자가 처리 |
| 모바일 대응 | `PlatformSwitcher.instance.IsPCPlatform` 조건 분기 | `MobileInputButton`이 같은 경로로 흡수 |
| 일시정지 | `Time.timeScale` 직접 조작 9곳 | `GamePauseController` 1곳 |

## 남은 한계 (스스로 확인한 것)
- 시스템 평가 후 구독 해제 불가 구조와 우회 전달 경로를 정리함 (이슈 #99). 그 이전 구조에는 문제가 있었음
