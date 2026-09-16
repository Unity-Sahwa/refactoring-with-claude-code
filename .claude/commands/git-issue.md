---
description: GitHub Issue를 라벨과 함께 작성해 등록한다.
---

# /git-issue

## 원칙
1. **라벨은 두 가지만 답한다 — "무엇을"(type)과 "어디를"(sys).**
   답이 늘어나면 고민하게 되고, 고민하면 안 붙인다.
2. **같은 정보는 한 곳에만 적는다.** 두 곳에 적으면 언젠가 어긋나고, 그때부턴 둘 다 못 믿는다.
3. **이름만 보고 뜻을 알아야 한다.** 설명을 읽어야 아는 라벨은 아무도 안 쓴다.

## type 판정
| 라벨 | 붙이는 때 | 예 |
|---|---|---|
| `type:bug` | 원래 되어야 할 게 안 되던 것을 고침 | NullReference, 입력 씹힘 |
| `type:feat` | 게임 동작이 늘거나 달라짐 | 새 기믹, 옵션 추가, 코드 파일 신규 생성 |
| `type:refactor` | 동작은 그대로, 이름·구조만 바뀜 | 변수명/파일명 변경, 클래스 분리, 죽은 코드 삭제 |
| `type:test` | 테스트·성능 측정용 코드 | PerfTestHub, ShaderWarmup |
| `type:docs` | 코드가 아닌 글 | 주석, README, CLAUDE.md |
| `type:asset` | 코드가 아닌 리소스 | SO, 프리팹, 머티리얼, 사운드 |

**판정 순서 — 위에서부터 내려가다 처음 걸리는 데서 멈춘다.**
1. 코드 파일이 아닌가? → 글이면 `docs`, 리소스면 `asset`
2. 테스트·측정용인가? → `test`
3. 빌드해서 나온 **게임 동작이 달라지나?** → 안 달라지면 `refactor`
4. 원래 됐어야 하는 건가? → 맞으면 `bug`, 새로 만든 거면 `feat`

- **type은 이슈당 하나만.** 두 개 붙이고 싶으면 이슈를 쪼갠다.
- **`bug`는 사람이 말해줘야 한다.** diff는 "뭐가 바뀌었나"만 보여주고 "원래 의도가 뭐였나"는 안 보여준다. 안 알려주면 `bug`로 판정하지 않는다.
- 표에 없는 경우는 판정 순서로 결정한다. **두 번 이상 헷갈린 것만** 표에 추가한다.

## sys 판정
`Assets/1.Code/Scripts/{X}System/` → `sys:{X}` (예: `InputSystem` → `sys:Input`)
`Editor` `Test`는 접미사가 없으므로 `sys:Editor` `sys:Test`.
시스템 폴더가 새로 생기면 `gh label create "sys:{X}" -c c5def5` 로 라벨을 추가한다.

## 절차
### 1. 수집
- `gh issue list --state open` — 중복 이슈가 있으면 알리고 멈춘다.
- `gh api "repos/:owner/:repo/milestones?state=all" --jq '.[] | "\(.number) \(.state) \(.title)"'`
- 작업 내용을 안 줬으면 `git status` / `git diff`로 추정해 제시하고 확인받는다.

### 2. 결정
- **제목**: `/git-commit`의 제목 규칙을 그대로 따른다.
   - 없던 것을 만드는 이슈: `[{시스템명}System] {대상}이 {무엇을 하게} {추가}`
   - 기존 것을 바꾸는 이슈: `[{시스템명}System] {대상}을 {이전}에서 {이후}로 {행위}`
   - 50자 이내, 마침표 없음. 대상과 행위 동사를 반드시 넣는다.
   - 필드명·클래스 내부 용어·영어 직역(`픽셀 예산`)을 그대로 옮기지 않는다.
     코드를 안 본 사람이 읽고 결과를 알 수 있게 수치나 조건으로 쓴다.
- **라벨**: type 1개 + sys 1개 이상.
- **Start date**: 이슈를 만드는 날.
- **Target date**: 언제까지 끝낼지 사용자와 정한다. 실제 종료일이 아니라 **목표일**이다. 실제 종료일은 Projects의 `Closed`가 자동으로 기록한다.
- **본문**: 생성 시점에는 쓰지 않는다. 이슈를 만들 때는 실제로 한 일이 아직 없기 때문이다.
  본문은 이슈를 닫을 때(6번) 그동안의 커밋을 근거로 채운다.

### 3. 초안 제시 → 승인
**한 번에 한 이슈만 제시한다.** 만들 이슈가 여러 개여도 첫 이슈만 보여주고 등록이 끝난 뒤 다음으로 넘어간다. 전체 개수만 한 줄로 알린다.
로컬에 md 파일을 만들지 않는다.

```
제목: [InputSystem] ...
라벨: type:refactor, sys:Input
기간: 2026-09-03 ~ 2026-09-05
본문: 없음 (닫을 때 채움)
```

### 4. 승인 후 등록
1. `gh issue create --title ... --body "" --label "type:x" --label "sys:y" --assignee Eoodyd [--milestone "..."]`
2. 프로젝트 보드에 올린다.
   `gh project item-add 3 --owner Unity-Sahwa --url <이슈 URL>` → 출력된 item id 사용
3. 날짜를 넣는다. (프로젝트 id `PVT_kwDOC7Pf_s4BUD6o`)
   Start date: `gh project item-edit --id <item id> --project-id PVT_kwDOC7Pf_s4BUD6o --field-id PVTF_lADOC7Pf_s4BUD6ozhBOm_k --date <시작일>`
   Target date: 같은 명령에 `--field-id PVTF_lADOC7Pf_s4BUD6ozhBOm_o`
4. 이슈 번호와 URL을 알린다.

### 6. 종료 시 본문·태그 라벨 작성 (`/git-commit`에서 이슈를 닫을 때 호출)
1. `git log --all --grep "Refs #<N>" --oneline`으로 그 이슈에 달린 커밋을 전부 모은다.
2. type 라벨로 파일 경로를 정한다 — `type:bug`면 `.claude/git/bug_issuetemplate.md`, 그 외 전부 `.claude/git/work_issuetemplate.md`. Read로 직접 연다.
3. 읽은 파일의 섹션 구조·순서를 유지하며, 모은 커밋들의 제목·본문을 근거로 각 섹션을 채운다. 커밋에 없는 내용은 추정해서 넣지 않는다.
4. 적을 내용이 없는 섹션은 통째로 지운다. HTML 주석은 등록 전에 지운다.
5. 모은 커밋들의 꼬리말 `#태그` 중 `Docs/Tags.md`의 **"커맨드·스킬" 범주에 속한 태그만** 골라 중복 제거하고 앞에 `tag:`를 붙인다. 시스템·개념·상태 범주 태그(예: `#camerasystem`, `#interface`, `#problemsolved`)는 `sys:` 라벨과 겹치거나 라벨 수만 늘리므로 넣지 않는다.
6. 채운 본문과 5번의 `tag:` 라벨 목록을 사용자에게 보여주고 승인받는다.
7. 승인 후 `gh issue edit <N> --body-file <임시파일>`로 본문을 갱신한다.
8. `gh label list`에 없는 `tag:` 라벨은 `gh label create "tag:xxx" -c <색상>`으로 먼저 만든다.
9. `gh issue edit <N> --add-label "tag:xxx"`로 5번 목록을 전부 붙인다.

## 금지
- 승인 없는 이슈·라벨 생성
- 마일스톤 생성
- 중복 이슈 생성 (기존 이슈 수정으로 되면 그렇게 한다)
- 승인 없는 이슈 본문 수정
