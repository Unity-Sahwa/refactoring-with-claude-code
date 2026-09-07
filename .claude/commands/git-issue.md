---
description: GitHub Issue를 라벨·마일스톤과 함께 작성해 등록한다.
---

# /git-issue

## 원칙
1. **라벨은 두 가지만 답한다 — "무엇을"(type)과 "어디를"(sys).**
   답이 늘어나면 고민하게 되고, 고민하면 안 붙인다.
2. **마일스톤은 달력이다. 마감일 없으면 만들지 않는다.**
3. **같은 정보는 한 곳에만 적는다.** 두 곳에 적으면 언젠가 어긋나고, 그때부턴 둘 다 못 믿는다.
4. **이름만 보고 뜻을 알아야 한다.** 설명을 읽어야 아는 라벨은 아무도 안 쓴다.

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
- **마일스톤**: 마감일이 정해진 게 있을 때만 붙인다. 열린 마일스톤이 없고 사용자가 기한을 주지 않으면 **붙이지 않는다.**
- **Start date**: 이슈를 만드는 날.
- **Target date**: 언제까지 끝낼지 사용자와 정한다. 실제 종료일이 아니라 **목표일**이다. 실제 종료일은 Projects의 `Closed`가 자동으로 기록한다.
- **본문**: 템플릿을 채운다. 적을 내용이 없는 섹션은 통째로 지운다 — 빈 껍데기를 남기지 않는다.
   `type:bug`면 `.claude/git/bug_issuetemplate.md`, 나머지는 모두 `.claude/git/work_issuetemplate.md`. HTML 주석(`<!-- -->`)은 안내문이므로 등록 전에 지운다.

### 3. 초안 제시 → 승인
**한 번에 한 이슈만 제시한다.** 만들 이슈가 여러 개여도 첫 이슈만 보여주고 등록이 끝난 뒤 다음으로 넘어간다. 전체 개수만 한 줄로 알린다.
로컬에 md 파일을 만들지 않는다.

```
제목: [InputSystem] ...
라벨: type:refactor, sys:Input
기간: 2026-09-03 ~ 2026-09-05
마일스톤: 없음 | <이름> (마감 2026-09-30)
본문:
  <템플릿 채운 내용>
```

### 4. 승인 후 등록
1. 마일스톤이 신규면 마감일과 함께 만든다.
   `gh api repos/:owner/:repo/milestones -f title="..." -f due_on="2026-09-30T00:00:00Z"`
2. `gh issue create --title ... --body-file <임시파일> --label "type:x" --label "sys:y" --assignee Eoodyd [--milestone "..."]`
3. 프로젝트 보드에 올린다.
   `gh project item-add 3 --owner Unity-Sahwa --url <이슈 URL>` → 출력된 item id 사용
4. 날짜를 넣는다. (프로젝트 id `PVT_kwDOC7Pf_s4BUD6o`)
   Start date: `gh project item-edit --id <item id> --project-id PVT_kwDOC7Pf_s4BUD6o --field-id PVTF_lADOC7Pf_s4BUD6ozhBOm_k --date <시작일>`
   Target date: 같은 명령에 `--field-id PVTF_lADOC7Pf_s4BUD6ozhBOm_o`
5. 이슈 번호와 URL을 알린다.

## 금지
- 승인 없는 이슈·마일스톤·라벨 생성
- 마감일 없는 마일스톤 생성
- 중복 이슈 생성 (기존 이슈 수정으로 되면 그렇게 한다)
