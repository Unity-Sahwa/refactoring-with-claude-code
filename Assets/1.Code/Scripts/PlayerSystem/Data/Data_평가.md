# PlayerSystem/Data 유지보수 평가 (수정 반영본)

대상: `Assets/1.Code/Scripts/PlayerSystem/Data`
기준: maintainability-evaluation (변경 파급 / 이해 비용 / 교체·삭제 / 과설계 + 유니티 함정)

## 0. 1차 평가 후 적용된 수정

- `InputBlockDataEntry` 죽은 코드 삭제.
- 동일 3클래스(`InputControl`/`Motion`/`Timing`) → `IntervalDataEntry` 하나로 통합. StateData 필드 6개 모두 `IntervalDataEntry[]`로 통일.
- 파일명≠타입명 2개 rename: `StateDataCategoryType.cs`→`StateEventCategory.cs`, `IHasTimingData.cs`→`IStartData.cs`.
- 통합으로 생긴 거짓 클래스 주석 수정(`IntervalDataEntry` 역할 주석을 공용 구간으로 정정).
- `IInputBlock`(거짓 주석 + 미사용) 삭제.

## 1. 판정표

| [판정] 핵심 질문 | 이유 + 피드백 |
|---|---|
| **[좋음]** 의존을 밖에서 받나 | 전부 데이터 컨테이너/SO. `new`·`Find`·싱글톤 없음 |
| **[좋음]** 남의 내부 파고드나 | `HitboxDataEntry`가 `combat.Damage`로 1단계 위임뿐. 깊은 체이닝 없음 |
| **[좋음]** 한 곳 고치면 끝나나 | 3클래스 → `IntervalDataEntry` 1개로 통합, 중복 제거 |
| **[좋음]** 흐름이 한 문장으로 | `StateData → GetData<T> → 카테고리별 데이터 제공` |
| **[좋음]** 클래스·메서드 한 줄 설명 | 이름·필드명 명확, 주석 정정 완료 |
| **[좋음]** 이름이 의도를 드러내나 | 파일명=타입명 정리 완료 |
| **[좋음]** 함수 짧고 단일작업 | `GetData`/`BuildDataMap` 짧고 한 일만 함 |
| **[좋음]** 격리 테스트 되나 | SO 순수 데이터라 그대로 주입 가능 |
| **[수용]** 새 종류 추가 시 기존 코드 수정 | 새 카테고리 추가 시 `enum`+`필드`+`BuildDataMap` 3곳 동시 수정 필요. 데이터 등록 구조상 불가피, 이득 대비 과해 수용 |
| **[좋음]** 비핵심 빠져도 핵심 도나 | `GetData`가 null·빈 배열 건너뜀 |
| **[좋음]** 지금 요구만큼만 만들었나 | 죽은 코드 `InputBlockDataEntry` 삭제 |
| **[검토]** 쓰지도 않는 추상화 없나 | `IPlayerEffect`·`IPlayerHitbox`·`ISkillMove` 각 구현 1개. 핸들러 디커플 의도면 유지, 아니면 과함 (다른 하위 시스템 평가 시 호출부 확인) |
| **[좋음]** 같은 일을 더 짧게 | 동일 클래스 3→1 |
| **[해당없음]** 구독·해제 짝 | Data에 이벤트 구독 없음 |
| **[해당없음]** 코루틴·async 정리 | 없음 |
| **[해당없음]** static·싱글턴 수명 | 없음 |
| **[해당없음]** 파괴 객체 vs null | 없음 |

## 2. 남은 사항

**나쁨: 없음.**

**검토 1건:** 단일구현 인터페이스 `IPlayerEffect`·`IPlayerHitbox`·`ISkillMove` — 핸들러가 이 시임으로 실제 디커플 이득을 보는지 호출부(Effect/Hitbox 하위 시스템) 평가 때 확인 후 유지/인라인 결정.

**별도 작업(합의됨):** `IMotionControl` 인터페이스 이름이 이제 공용 구간(입력·타이밍 포함)을 가리키므로 의미와 안 맞음. `IInterval`/`IGate` 등으로 rename 권장 — 이번 범위 밖.

**보류 결정:** `startProgress`+`duration`을 MinMax 범위 슬라이더로 바꾸는 건 YAGNI로 보류(현재 `[Range(0,1)]` 슬라이더로 충분, 범위 초과 실수가 잦아지면 그때 커스텀 드로어 도입).
