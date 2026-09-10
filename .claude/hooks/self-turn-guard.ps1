[Console]::InputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
# 모델이 자기 응답 뒤에 오지 않은 사용자 발화(줄 첫머리 user)를 이어 쓴 경우를 잡는다.
# Stop:        누출분을 잘라낸 응답을 다시 보여주고 로그에 남긴다. 종결 표식 [끝] 누락은 경고만 한다.
# PreToolUse:  직전 응답에 누출이 있으면 도구 실행을 막는다.
$raw = [Console]::In.ReadToEnd()
try { $payload = $raw | ConvertFrom-Json } catch { exit 0 }

$event = $payload.hook_event_name
if ($payload.stop_hook_active) { exit 0 }

# 마지막 응답 텍스트 — Stop은 페이로드에 있고, 그 외에는 transcript에서 읽는다.
$text = $payload.last_assistant_message
if (-not $text -and $payload.transcript_path -and (Test-Path $payload.transcript_path)) {
    $lines = Get-Content -LiteralPath $payload.transcript_path -Encoding UTF8
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        try { $o = $lines[$i] | ConvertFrom-Json } catch { continue }
        if ($o.type -ne 'assistant') { continue }
        $t = ($o.message.content | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) -join "`n"
        if ($t.Trim()) { $text = $t; break }
    }
}
if (-not $text) { exit 0 }

$leak = [regex]::Match($text, '(?m)^[ \t]*user')

if (-not $leak.Success) {
    if ($event -eq 'Stop' -and $text.TrimEnd() -notmatch '\[끝\]$') {
        @{ systemMessage = "종결 표식 [끝]이 없음." } | ConvertTo-Json -Compress
    }
    exit 0
}

$head = $text.Substring(0, $leak.Index).TrimEnd()
$fake = $text.Substring($leak.Index).TrimEnd()

if ($event -eq 'PreToolUse') {
    @{ hookSpecificOutput = @{ hookEventName = 'PreToolUse'; permissionDecision = 'deny'
                               permissionDecisionReason = '직전 응답에 이어쓴 user 턴이 있어 실행을 막음.' } } |
        ConvertTo-Json -Depth 5 -Compress
    exit 0
}

# 로그 — 문제 응답, 누출분, 직전 사용자 발화를 남긴다.
$prev = ''
if ($payload.transcript_path -and (Test-Path $payload.transcript_path)) {
    $lines = Get-Content -LiteralPath $payload.transcript_path -Encoding UTF8
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
        try { $o = $lines[$i] | ConvertFrom-Json } catch { continue }
        if ($o.type -ne 'user') { continue }
        $c = $o.message.content
        $t = if ($c -is [string]) { $c } else { ($c | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) -join "`n" }
        if ($t -and $t -notmatch '<system-reminder>') { $prev = $t; break }
    }
}
$logDir = Join-Path $PSScriptRoot '..\logs'
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }
$record = [ordered]@{
    time = (Get-Date).ToString('o'); session = $payload.session_id; project = $payload.cwd
    prev = $prev; head = $head; fake = $fake
}
Add-Content -LiteralPath (Join-Path $logDir 'self-answered-turns.jsonl') `
            -Value ($record | ConvertTo-Json -Depth 5 -Compress) -Encoding UTF8

@{ systemMessage = "$head`n`n(user 문제 발생)" } | ConvertTo-Json -Compress
exit 0
