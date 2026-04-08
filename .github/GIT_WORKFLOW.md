# Git Workflow (GitHub Flow 기반)

## 작성 의도
- 흐름이 단순하고 규칙이 적어 개인 프로젝트에 적합
- 복잡한 브랜치 관리 대신 통합 과정의 충돌 같은 핵심 문제에 집중

---

## 브랜치 구조

```
main
└── develop
    ├── feature/scope-description
    └── bugfix/scope-description
```

| 브랜치 | 용도 |
|--------|------|
| `main` | 마일스톤 완료 시 Merge. 직접 수정 금지. 빌드 영상/문서 작성 기준 |
| `backup/original` | 리팩토링 전 원본 스냅샷. 참고용, 수정 금지 |
| `develop` | 통합 브랜치. 작업은 항상 하위 브랜치에서 진행 |
| `feature/scope-description` | 기능 추가 / 리팩토링 / 최적화 / Task. develop에서 파생, Merge 후 삭제 |
| `bugfix/scope-description` | 버그 수정. develop 또는 main에서 파생, Merge 후 삭제 |

---

## 브랜치 네이밍 규칙

- **영문 소문자 + 하이픈** 사용. 한글·특수문자·이슈번호 금지
- 단위: 클래스 단위가 아닌 **시스템/기능 범위** 단위로 명명
- 하나의 브랜치에 여러 이슈가 포함될 수 있음 → 커밋별로 이슈 참조

| Issue 타입 | 브랜치 |
|-----------|--------|
| Feature | `feature/scope-description` |
| Bug | `bugfix/scope-description` |
| Task | `feature/scope-description` |

**예시:**
```
feature/core-infrastructure     ← GameManager, DIContainer 등 핵심 인프라
feature/player-refactoring      ← Player 관련 시스템 전체
bugfix/jump-input-duplicate     ← 점프 중복 입력 버그
feature/game-state-system       ← 게임 상태 관리 시스템
```

---

## 작업 흐름

1. Issue 생성 (Bug / Feature / Task 템플릿)
2. develop에서 브랜치 파생
3. 브랜치에서 작업 — 커밋은 타입별로 분리
4. develop으로 Merge → 브랜치 삭제
5. 마일스톤 완료 시 develop → main Merge

---

## 커밋 분리 방식

같은 브랜치에서 작업을 마친 후, 커밋만 타입별로 나눠서 기록:

```
feature/player-refactoring 브랜치에서

refactor(Player): 상태머신 클래스 분리 (Closes #5)
fix(Player): 점프 중 대시 입력 무시 안되는 버그 수정 (Closes #6)
docs(Player): 상태머신 구조 주석 추가 (Refs #5)
```

---

## main 브랜치 규칙
- 직접 수정 금지 → Merge 충돌 예방
- develop에서만 Merge
- 마일스톤 완료 시: 빌드 영상 제작 + 문서 작성

---

## 참고 문서
- 커밋 메시지 규칙: `COMMIT_CONVENTIONS.md`
- PR 작성 규칙: `PR_CONVENTION.md`
