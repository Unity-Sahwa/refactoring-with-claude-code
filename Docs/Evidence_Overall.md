# 리팩토링 근거 — 전체 시스템 설계 (레거시 대비 실측)

**주제: 이름 공간과 시스템 경계 만들기.** 분류만 되던 폴더를 계약을 노출하는 시스템으로 바꿈.

여기 적힌 것은 양쪽 코드 실측이거나 본인 답변임. 추측은 적지 않음.
Enemy·Gimmick은 본인 작업 범위가 아니므로 비교 대상에서 제외함.

## 총량 지표 (실측)
| 지표 | before | after |
|---|---|---|
| 전체 파일 수 | 134 | 274 |
| 최상위 구분 | 폴더 10개 (`Player`·`Enemy`·`Gimmick`·`UI`·`Manager`·`Camera`·`Object`·`Struct`·`Interface`·`Timeline`) | 시스템 16개 |
| `namespace` 선언 파일 | **0 / 134** | **270 / 274** |
| 인터페이스 총수 | 2 (`IDamageable`·`IEvent`) | 53파일 |
| 싱글톤 클래스 | 26 | 0 |

## before: 폴더는 있었지만 경계는 없었음
| 폴더 | 파일 | 줄 |
|---|---|---|
| `Player` | 36 | 8,854 |
| `Enemy` | 28 | 4,172 |
| `UI` | 13 | 2,212 |
| `Gimmick` | 38 | 1,411 |
| `Manager` | 8 | 1,355 |
| `Camera` | 3 | 862 |
| `Object` | 4 | 472 |
| `Struct` | 2 | 150 |
| `Interface` | 2 | 29 |
| `Timeline` | 0 | 0 |

문제
- `namespace`가 **한 파일도 없음**. 모든 타입이 전역 이름 공간에 있어 이름 충돌을 파일명 규칙으로만 피함
- 폴더가 분류일 뿐 경계가 아님. `MenuUI`(UI)가 `MaskChange`(Player)·`CameraController`(Camera)를 직접 참조하고,
  `TimelineHelper`(Player)가 `MenuUI`·`MouseSettingUI`(UI)를 참조함
- `Interface` 폴더에 인터페이스가 2개뿐. 폴더는 만들었으나 계약 기반 설계가 자리잡지 않은 상태
- `Manager` 폴더가 `SaveManager`·`SceneSwitcher`·`LanguageManager`·`TextManager`·`CoroutineManager` 등
  성격이 다른 전역 객체의 집합소 역할
- `Timeline` 폴더는 비어 있음

## after: 시스템 16개, 각자 계약을 노출
| 시스템 | 파일 | 줄 | 노출 인터페이스 |
|---|---|---|---|
| PlayerSystem | 81 | 3,691 | 26 |
| GimmickSystem | 34 | 1,685 | 1 |
| UISystem | 28 | 1,380 | 2 |
| EnemySystem | 25 | 2,682 | 0 |
| CameraSystem | 24 | 1,249 | 8 |
| SaveSystem | 15 | 767 | 3 |
| InputSystem | 12 | 396 | 6 |
| Editor | 11 | 777 | 0 |
| LanguageSystem | 11 | 644 | 1 |
| AudioSystem | 9 | 606 | 1 |
| CombatSystem | 6 | 360 | 1 |
| GameStateSystem | 6 | 168 | 2 |
| DISystem | 4 | 278 | 1 |
| SettingsSystem | 3 | 77 | 1 |
| PlatformSystem | 2 | 115 | 0 |

- 파일 274개 중 270개가 `namespace Refactoring` 아래에 있음
- 시스템 간 연결은 인터페이스 또는 채널을 통함.
  채널 5종: `AudioChannel`·`HitChannel`·`PlayerStateEventChannel`·`PlayerStateTriggerChannel`·`PlayerCurrentStateChannel`
- 배선은 `AttributeInjector`가 씬 `Awake`에서 자동 수행 (`Evidence_DI.md` 참고)

## 시스템 경계가 실제로 작동한 사례
- `PlayerSystem`이 카메라를 직접 조작하지 않음. `CameraSystem`이 `IPlayerStateEventSubscriber`로 상태 이벤트를 구독해
  흔들림·줌을 스스로 처리함 (before는 `PlayerCameraEffect`가 플레이어 안에 있었음)
- `CombatSystem`이 `DamageInfo`/`IDamageable`만 정의하고, 공격자·피격자는 서로의 구체 타입을 모름
- `UISystem`이 게임 상태를 `IGameStateController`로만 건드림 (before는 `MenuUI`가 `Time.timeScale`과 플레이어를 직접 조작)

## 남은 한계 (스스로 확인한 것)
- `EnemySystem`(25파일)과 `PlatformSystem`은 노출 인터페이스가 0개. 계약 없이 구체 타입으로 연결됨
- `GimmickSystem`은 34파일에 인터페이스 1개. 시스템 크기 대비 계약이 적음
- `Editor`는 성격상 인터페이스가 필요 없음

## 정리
before와 after의 파일 수는 134 → 274로 늘었지만, 늘어난 것은 **계약과 경계**임.
- 이름 공간: 0개 → 270개 파일이 네임스페이스 아래
- 계약: 인터페이스 2개 → 53파일
- 전역 접근: 싱글톤 26개 / 접근 230회 → 0개
