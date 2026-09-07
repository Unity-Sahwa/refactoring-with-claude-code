[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
# 규칙 파일(CLAUDE.md / .claude/)이 수정되면 AI에게 변경 이유를 기록하라고 지시한다.
# 훅은 대화 맥락을 모르므로 기록 자체는 AI가 한다.
$raw = [Console]::In.ReadToEnd()
try { $payload = $raw | ConvertFrom-Json } catch { exit 0 }

$path = $payload.tool_input.file_path
if (-not $path) { exit 0 }

$normalized = $path.Replace('\', '/')
if ($normalized -notmatch '(^|/)CLAUDE\.md$' -and $normalized -notmatch '(^|/)\.claude/') { exit 0 }

$context = @"
규칙 파일 '$normalized' 이(가) 수정됐다. Docs/RuleChangeLog.md 맨 위 표에 행을 추가한다.
없으면 헤더 '| 날짜 | 파일 | 무엇을 | 왜 |' 로 파일을 만든다.
'왜'는 이번 대화에서 확인된 이유만 쓴다. 추측이면 쓰지 않는다.
"@

$out = @{
  hookSpecificOutput = @{
    hookEventName    = "PostToolUse"
    additionalContext = $context
  }
}
$out | ConvertTo-Json -Depth 5 -Compress
