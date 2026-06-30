# PlayerSystem/Swap 유지보수 평가

대상: `Assets/1.Code/Scripts/PlayerSystem/Swap` (4개)
기준: maintainability-evaluation (변경 파급 / 이해 비용 / 교체·삭제 / 과설계 + 유니티 함정)

## 1. 판정표

| [판정] 핵심 질문 | 이유 + 피드백 |
|---|---|
| **[좋음]** 의존을 밖에서 받나 | `[Inject] List<PlayerCharacter>`. 싱글톤·Find 없음 |
| **[좋음]** 남의 내부 파고드나 | `character.Type`, `.transform` 1단계 |
| **[좋음]** 한 곳 고치면 끝나나 | 스왑 로직 한 곳 |
| **[좋음]** 흐름이 한 문장으로 | `다른 타입 캐릭터 찾기 → 위치 복사 → active 토글 → 통지` |
| **[좋음]** 클래스·메서드 한 줄 설명 | 책임 주석 보강 완료 |
| **[좋음]** 격리 테스트 | Unity SetActive라 엔진 경계(불가피) |
| **[수용]** 새 종류 추가 시 | `Type != current.Type`로 다른 타입 첫 캐릭터 선택 = 2캐릭터/2타입 전제. 주석에 "2가지만 사용" 명시. 3+ 되면 로직 수정 필요 |
| **[좋음]** 비핵심 빠져도 핵심 도나 | `nextCharacter` null 가드 + 경고 추가(수정 완료) |
| **[좋음]** 지금 요구만큼만 | 군더더기 없음 |
| **[좋음]** 쓰지도 않는 추상화 | 인터페이스 3분할이 모범 ISP — 소비자별로 Swap/현재/통지를 따로 의존(주석에 의도 명시) |
| **[해당없음]** 구독·해제 / 코루틴 / static | Switcher는 이벤트 발행만(인스턴스 event라 누수 없음). 구독자(Mover)는 자기 해제 확인됨 |

## 2. 문제와 개선 방향

**좋은 점:** ISP 3분할(Swappable/CurrentProvider/Notifier)로 소비자가 필요한 것만 의존 — 교과서적. 스왑 시 위치 이어받기·통지 깔끔.

**처리 결과:**

1. **[해결] `nextCharacter` null 미가드** → 다음 캐릭터 없으면 경고 후 `return` 추가(NRE 방지).
2. **[해결] 책임 주석 + 네이밍** → 클래스 책임 1줄 추가, `_characters`·`_currentCharacter`로 `_` 컨벤션 통일.
3. **[수용] 2캐릭터/2타입 전제** — 현재 스코프(Human/Animal)에 맞음. 캐릭터 종류가 늘면 선택 로직 재검토.

## 3. 결론

나쁨 0(해결 완료). ISP 설계 모범.
별도 메모: `[SerializeField]` 필드 네이밍이 코드베이스 전체에서 불일치(`stateType` vs `_moveSpeed`) — 추후 컨벤션 일괄 점검 대상.
