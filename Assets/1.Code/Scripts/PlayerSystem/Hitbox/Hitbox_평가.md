# PlayerSystem/Hitbox 유지보수 평가

대상: `Assets/1.Code/Scripts/PlayerSystem/Hitbox` (8개)
기준: maintainability-evaluation (변경 파급 / 이해 비용 / 교체·삭제 / 과설계 + 유니티 함정)

## 1. 판정표

| [판정] 핵심 질문 | 이유 + 피드백 |
|---|---|
| **[좋음]** 의존을 밖에서 받나 | Handler·Provider 전부 `[Inject]`. 싱글톤·Find 없음. Reporter의 `GetComponentsInChildren`는 자기 자식이라 정상 |
| **[좋음]** 남의 내부 파고드나 | `source.gameObject` 1단계뿐. 깊은 체이닝 없음 |
| **[좋음]** 한 곳 고치면 끝나나 | Overlap=Reporter, 풀=Provider, 켜기=Handler로 책임 분리 |
| **[좋음]** 흐름이 한 문장으로 | `이벤트 → Rent → 부착 → Setup → 활성화 → 코루틴(지속) → Finish → Return` |
| **[좋음]** 클래스·메서드 한 줄 설명 | 책임 주석 2개 보강 완료. 전반 충실 |
| **[좋음]** 함수 짧고 단일작업 | `GetCapsuleWorld` 등 길어도 한 가지 일(수학). 정당 |
| **[좋음]** 격리 테스트 되나 | Handler·Provider 주입 기반. Reporter는 `Physics.Overlap`(엔진 경계)이라 불가피 |
| **[수용]** 새 종류 추가 시 기존 코드 수정 | `OverlapCollider`/`OnDrawGizmos` switch가 Box/Capsule/Sphere 분기 → 콜라이더는 닫힌 집합이라 불가피 |
| **[좋음]** 비핵심 빠져도 핵심 도나 | Rent null·부착점 없음·reporter null 모두 가드. 견고 |
| **[좋음]** 지금 요구만큼만 만들었나 | `_targetMask`도 "적 핸들러 생기면 추상화"로 미리 안 만듦(YAGNI 준수) |
| **[검토]** 쓰지도 않는 추상화 없나 | 단일구현 인터페이스 3개(Provider/Reporter/AttachPoint). Reporter는 적·플레이어 공용 의도, AttachPoint는 List 주입용이라 대체로 정당. Provider는 호출부 핸들러 1곳 |
| **[좋음]** 같은 일을 더 짧게 | 군더더기 없음 |
| **[좋음]** 구독·해제 짝 | Awake 구독 ↔ OnDestroy 해제(영속 핸들러라 맞음). null 가드까지. States #1 PASS |
| **[좋음]** 코루틴 정리 | OnDestroy 전 코루틴 Stop, Reset에서 비-untilFinish Stop. 깔끔 |
| **[나쁨(약)]** 파괴 객체 vs null | 2장 1번 |
| **[나쁨(약)]** GC/문자열 | 2장 2번 |
| **[해당없음]** static·싱글턴 수명 | 없음 |

## 2. 문제와 개선 방향

**좋은 점:** 책임 분리(Reporter/Provider/Handler) 깔끔. `OverlapXxxNonAlloc` + 버퍼 재사용으로 GC 억제, MeshRenderer·Reporter를 생성 시 1회 캐싱해 런타임 순회 제거. 코루틴·구독 정리 철저. `_targetMask` 추상화를 미리 안 만든 YAGNI 준수.

**문제 (둘 다 드문 엣지 — 결론: 필수 수정 아님):**

1. **[나쁨 약] `HitboxProvider` 파괴 타이밍 엣지 2개**
   - `Return`: `_instances.ContainsValue`는 참조 비교라 파괴된 인스턴스가 dict에 남아 있으면 true → 파괴 객체에 `SetActive` 시도. Provider가 수명 소유라 실전 거의 불가능 → **스킵.**
   - `LoadAndBuild`의 `handle.Completed` 람다: 로딩 중 Provider 파괴 시 파괴된 transform 밑에 `Instantiate` 시도. 콜백 진입부 `if (this == null) return;` 한 줄로 막을 수 있음 → 씬 전환이 잦으면 선택 적용, 아니면 스킵.

2. **[나쁨 약] `_targetMask` 레이어명 문자열 하드코딩** (PlayerHitboxHandler.cs:34) — `LayerMask.GetMask("Enemy","Gimmick")`는 이름 오타·변경 시 0 반환. 단 "공격이 안 맞음"으로 플레이 즉시 티나 디버깅 빠름 → **스킵.**

## 3. 결론

필수 수정 0개. `1b`의 한 줄 가드만 선택. 이 하위 시스템은 전반적으로 매우 견고.
States #1(SO 구독 잔존) 교차확인 → **PASS 확정**.
