# PlayerSystem/States 유지보수 평가

대상: `Assets/1.Code/Scripts/PlayerSystem/States` (Old 제외, 11개)
기준: maintainability-evaluation (변경 파급 / 이해 비용 / 교체·삭제 / 과설계 + 유니티 함정)

## 1. 판정표

| [판정] 핵심 질문 | 이유 + 피드백 |
|---|---|
| **[좋음]** 의존을 밖에서 받나 | `PlayerStateMachine`이 `[Inject]`로 raiser·triggerSubscriber 받음. StateData는 인스펙터 주입. `GetComponent<PlayerCharacter>()`는 같은 오브젝트 형제 컴포넌트라 허용 범위 |
| **[좋음]** 남의 내부 파고드나 | `character.GetCharacterComponent<Animator>()` 1단계뿐. 깊은 체이닝 없음 |
| **[좋음]** 한 곳 고치면 끝나나 | 전환 규칙이 `TryGetNextState` switch 한 곳에 모임 |
| **[좋음]** 흐름이 한 문장으로 | `trigger → SendTrigger → TryGetNextState → TransitionTo → Runner.Enter/Update → EventChannel.Raise → 구독자` |
| **[좋음]** 클래스·메서드 한 줄 설명 | 이름·주석 명확 |
| **[좋음]** 함수 짧고 단일작업 | `TryGetNextState`는 길지만 "전환표"라는 한 가지 일. 정당 |
| **[좋음]** 격리 테스트 되나 | StateMachine·Runner 모두 의존성 주입받아 가짜 교체 가능 (단 `AnimationTracker`는 실제 `Animator` 필요 — 엔진 경계라 불가피) |
| **[수용]** 새 종류 추가 시 기존 코드 수정 | 새 상태·트리거 추가 시 `TryGetNextState` switch 수정 필요(OCP 위반처럼 보임). 하지만 전환표는 한곳에 모으는 게 정석 — 흩으면 더 나빠짐. 주석에 의도 명시됨. 수용 |
| **[좋음]** 비핵심 빠져도 핵심 도나 | `BuildStates` null 데이터 건너뜀, `TransitionTo` 미등록 상태 가드 |
| **[좋음]** 지금 요구만큼만 만들었나 | 죽은 `IMoveInputReceiver` 이미 삭제됨. 군더더기 없음 |
| **[정당]** 쓰지도 않는 추상화 없나 | 인터페이스 4개 각 구현 1개지만, raiser/subscriber 분리는 ISP 목적이 명확(StateMachine은 subscriber만, Runner는 raiser만 봄). 정당 |
| **[좋음]** 같은 일을 더 짧게 | 군더더기 없음 |
| **[좋음]** 구독·해제 짝 | StateMachine `OnEnable`↔`OnDisable`에서 trigger 구독/해제 짝 맞음 |
| **[교차확인]** SO·static 수명 | 채널 자체는 정상(Unsubscribe·OnEnable 초기화 보유). 도메인 리로드 끔 환경이라 진짜 위험은 "구독자가 OnDisable에서 해제 안 함" — 구독자(Effect/Hitbox 등) 평가 때 검증 |
| **[해당없음]** 코루틴·async 정리 | 없음 |
| **[좋음]** 파괴 객체 vs null | `PlayerStateMachine.Awake`에서 PlayerCharacter null 시 `return`으로 중단(BuildStates NRE 방지) — 수정 완료 |

## 2. 문제와 개선 방향

**좋은 점:** 이벤트 채널(SO)로 쏘는/받는 쪽 완전 분리. 전환 규칙을 한 switch에 모아 한눈에 파악. raiser/subscriber 인터페이스 분리로 각자 필요한 것만 의존.

**문제 (해결 완료):**

1. ~~**[나쁨] `PlayerStateMachine.Awake`의 null 처리가 로그만 함**~~ → **해결**. null 시 `return;` 추가해 `BuildStates` NRE 방지(line 23).

2. ~~**[이해비용] `PlayerStateEventChannel`**~~ → **해결**. 책임 주석 추가 + `[CreateAssetMenu]` 메뉴명 오타 `Chennal`→`Channel` 수정.

**남은 사항 (이 파일 밖 책임 — 교차 확인):**

- **[교차확인] SO 이벤트 채널 구독 잔존** — 채널 자체는 정상(`Unsubscribe*`·`OnEnable` 초기화 보유). 단 **도메인 리로드 끔** 환경에선 `OnEnable`이 플레이 시작 때 안 불리므로, 안전은 전적으로 "구독자가 `OnDisable`에서 해제"에 달림. 플레이 종료 시 `OnDisable`은 항상 불리므로 구독자만 짝을 맞추면 누적 없음. → 구독자(Effect/Hitbox/Animation 핸들러) 평가 때 검증.

## 3. 교차 확인 필요 (다른 하위 시스템)

- `IPlayerStateEventSubscriber` 구독자들이 `OnDisable`에서 해제하는지 (1번과 연결).
- 단일구현 인터페이스(raiser/subscriber)의 디커플 실효 — 현재는 ISP 목적 명확해 정당으로 판정.
