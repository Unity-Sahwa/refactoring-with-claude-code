# 규칙 변경 기록

CLAUDE.md, `.claude/` 아래 커맨드·설정·훅이 바뀔 때마다 한 행씩 위에 쌓는다.
`왜`는 대화에서 확인된 이유만 적는다. 추측은 적지 않는다.

| 날짜 | 파일 | 무엇을 | 왜 |
|---|---|---|---|
| 2026-09-10 | CLAUDE.md | 표현 규칙 4개 추가(전문용어·함축어 금지, 조사·보조어 생략 금지, 비유 금지, 과장 수식어 금지) | 사용자가 에이전트 설명의 전문용어·함축어·비유·과장 수식어 때문에 이해가 어렵다고 함 |
| 2026-09-10 | .claude/hooks/rule-verify.ps1 | `$OutputEncoding`을 BOM 있는 UTF8에서 `New-Object System.Text.UTF8Encoding $false`(BOM 없음)로 교체 | BOM 바이트가 claude -p 파이프 입력 앞에 붙어 검증 세션이 인코딩 깨짐으로 응답, 편집이 계속 deny됐다 |
| 2026-09-07 | .claude/hooks/rule-change-log.ps1 | 규칙 파일 수정 시 이 기록을 남기라고 AI에게 지시하는 훅 신규 | 훅은 대화 맥락을 모르므로 이유는 AI가 써야 한다 |
| 2026-09-07 | .claude/settings.json | PostToolUse 훅(matcher `Edit|Write`) 등록 | 이유가 안 남으면 왜 바꿨는지 모른 채 이전 수정으로 되돌아간다 |
| 2026-09-07 | .claude/commands/git-issue.md | 제목 규칙을 `요약` 한 단어에서 /git-commit 제목 규칙 전문으로 교체 | `요약`만 적혀 있어 필드명 직역 제목(`픽셀 예산으로 제한`)이 그대로 통과했다 |
