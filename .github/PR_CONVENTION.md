# Pull Request Convention

## 작성 의도
- PR이 쌓일수록 리뷰 맥락을 잃는 상황을 줄이기 위함
- 제목·본문 구조를 통일해 변경 의도를 빠르게 파악하기 위함

---

## PR 제목 형식

```
[타입] 스코프: 제목
```

| 타입 | 용도 |
|------|------|
| `[feat]` | 기능 구현/추가 |
| `[fix]` | 버그 수정 |
| `[refactor]` | 리팩토링 |
| `[optimize]` | 성능 최적화 |
| `[docs]` | 문서 작성 |
| `[chore]` | 빌드, 패키지 등 기타 |

**예시:**
```
[refactor] CoreInfra: DIContainer 및 GameManager 이벤트 구조 리팩토링
[feat] Player: 대시 쿨타임 시스템 추가
[fix] Enemy: 보스 2페이즈 전환 버그 수정
```

---

## PR 본문 구조

```markdown
## 개요
<!-- 이 PR이 무엇을 하는지 한 문장으로 설명 -->

## 변경 사항
<!-- 주요 변경 내용을 항목별로 기술 -->
- 
- 

## 관련 이슈
<!-- Closes / Fixes / Ref 키워드 사용 -->
Closes #

## 참고 사항
<!-- 리뷰어가 알아야 할 맥락, 주의할 점, 미완성 사항 등 (없으면 생략) -->
```

---

## 규칙

- **브랜치 방향**: feature/* → develop, develop → main
- **Merge 방식**: Squash Merge 금지. 커밋 이력 보존을 위해 일반 Merge 사용
- **Self-review**: Merge 전 본인이 diff를 한 번 확인
- **이슈 연결 필수**: 관련 이슈가 없으면 PR을 올리지 않음
- **브랜치 삭제**: Merge 완료 후 작업 브랜치 삭제

---

## 이슈 연결 키워드

| 키워드 | 동작 |
|--------|------|
| `Closes #123` | main에 Merge되면 이슈 자동 종료 |
| `Fixes #123` | Closes와 동일 (버그 수정 시 주로 사용) |
| `Ref #123` | 이슈를 참고만 함, 종료되지 않음 |

---

## 예시

```markdown
## 개요
DIContainer와 GameManager의 의존성 주입 구조를 인터페이스 기반으로 리팩토링함.

## 변경 사항
- IInjectable, IInjectTarget, IInjectRequester 인터페이스 분리
- DIContainer를 싱글턴에서 명시적 등록 방식으로 전환
- GameManager가 IGameStateEvent를 통해 상태 이벤트를 중계하도록 수정

## 관련 이슈
Closes #3
Closes #5

## 참고 사항
RefactoringScene은 테스트용이며 실제 게임 씬에는 미적용 상태.
```
