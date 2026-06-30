# PlayerSystem/Movement 유지보수 평가

대상: `Assets/1.Code/Scripts/PlayerSystem/Movement` (Old 제외, 8개)
기준: maintainability-evaluation (변경 파급 / 이해 비용 / 교체·삭제 / 과설계 + 유니티 함정)

## 1. 판정표

| [판정] 핵심 질문 | 이유 + 피드백 |
|---|---|
| **[좋음]** 의존을 밖에서 받나 | Mover 전부 `[Inject]`(옵션 플래그까지), 소스는 생성자 주입. 단 `Camera.main`은 전역 조회(2장 1번) |
| **[좋음]** 남의 내부 파고드나 | `frame.Controller.isGrounded` 등은 API 사용. 깊은 객체 체이닝 없음 |
| **[좋음]** 한 곳 고치면 끝나나 | 속도 합산이 Mover.Update 한 곳. 각 소스 독립 |
| **[좋음]** 흐름이 한 문장으로 | `입력→방향 / 소스별 속도 합산 → CharacterController.Move 1회` |
| **[좋음]** 클래스·메서드 한 줄 설명 | 전 파일 책임+왜 주석 충실. 최상 |
| **[좋음]** 함수 짧고 단일작업 | 전부 짧고 단일 책임 |
| **[좋음]** 격리 테스트 되나 | 속도 소스가 순수 C#+인터페이스 주입 → 테스트 최적 |
| **[좋음]** 새 종류 추가 시 기존 코드 수정 | 모범 OCP — 새 속도 소스 = `IVelocitySource` 구현 + Awake에 `.Add` 한 줄 |
| **[좋음]** 비핵심 빠져도 핵심 도나 | `[Inject(true)]` 옵션 + null 폴백(구독자 없으면 항상 허용) 철저 |
| **[좋음]** 지금 요구만큼만 만들었나 | `IVelocitySource` 구현 3개라 추상화에 실수요 있음. 군더더기 없음 |
| **[좋음]** 같은 일을 더 짧게 | 군더더기 없음 |
| **[좋음]** 구독·해제 짝 | 소스 ctor↔Dispose, Mover 이벤트 Awake↔OnDestroy 모두 짝 맞음 (States #1 PASS) |
| **[해당없음]** 코루틴·async 정리 | 없음 |
| **[나쁨(약)]** static·전역 수명 | 2장 1번 — `Camera.main` 캐싱 |
| **[수용]** 매 프레임 비용 | 2장 2번 — 이동 중 매 프레임 Move 트리거 발행 |
| **[좋음]** 파괴 객체 vs null | Update가 controller/transform null 가드, Setup이 character 가드 |

## 2. 문제와 개선 방향

**좋은 점:** `IVelocitySource`로 속도 요인을 부품화 → 새 이동 추가가 인터페이스 구현 한 개로 끝(모범 OCP). 순수 C# 소스라 테스트 쉬움. 옵션 주입 + null 폴백으로 부분 부재에 견고. `CharacterController.Move`로 이동을 한 번에 적용해 충돌 처리 위임. 주석 품질 최상.

**문제:**

1. **[나쁨 약] `Camera.main` 1회 캐싱** (PlayerCharacterMover.cs:57) — 런타임에 메인 카메라가 바뀌면 `_camera`가 stale이라 이동 방향이 옛 카메라 기준. 파괴되면 Unity-null이라 이동만 멈춤(크래시 X). → 락온/시네머신 도입 시 카메라 참조 주입/갱신 권장. 단일 카메라면 스킵.

2. **[수용] 이동 중 매 프레임 Move 트리거 발행** (WalkVelocitySource.cs:40) — `_canMove`(MoveControl) + 입력이 있는 동안 매 프레임 `RaiseTrigger(Move)`. 이 트리거는 **죽은 게 아님**: 스킬 마지막 구간에서 MoveControl을 켜 이동입력으로 Locomotion 복귀를 허용하는 용도라 필요함. 매 프레임 발행 비용은 미미(델리게이트+switch, 할당 없음)해 **수용 결정**(고치지 않음). 거슬리면 엣지 감지(`_wasMoving` bool)로 이동 시작 시 1회만 발행 가능.

## 3. 결론

필수 수정 0개. 가장 견고한 하위 시스템. 1번은 카메라 확장 시점에 판단, 2번은 수용.
