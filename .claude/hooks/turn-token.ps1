$ErrorActionPreference = 'Stop'
$raw = [Console]::In.ReadToEnd()
try { $inp = $raw | ConvertFrom-Json } catch { exit 0 }
$path = $inp.transcript_path
if (-not $path -or -not (Test-Path $path)) { exit 0 }

$lines = Get-Content -LiteralPath $path -Encoding UTF8

# sum assistant usage after the last real user turn
$start = 0
for ($i = $lines.Count - 1; $i -ge 0; $i--) {
    try { $d = $lines[$i] | ConvertFrom-Json } catch { continue }
    if ($d.type -eq 'user' -and -not $d.isMeta) {
        $c = $d.message.content
        $isToolResult = $false
        if ($c -is [array]) {
            foreach ($b in $c) { if ($b.type -eq 'tool_result') { $isToolResult = $true } }
        }
        if (-not $isToolResult) { $start = $i; break }
    }
}

$in = 0; $out = 0; $cr = 0; $cw = 0; $n = 0
$seen = @{}
for ($i = $start; $i -lt $lines.Count; $i++) {
    try { $d = $lines[$i] | ConvertFrom-Json } catch { continue }
    $u = $d.message.usage
    if (-not $u) { continue }
    $id = $d.message.id
    if ($id -and $seen.ContainsKey($id)) { continue }
    if ($id) { $seen[$id] = $true }
    $n++
    $in += [int]$u.input_tokens
    $out += [int]$u.output_tokens
    $cr += [int]$u.cache_read_input_tokens
    $cw += [int]$u.cache_creation_input_tokens
}
if ($n -eq 0) { exit 0 }

$sum = $in + $out + $cr + $cw
$msg = "Tokens: {0:N0}(in {1:N0} / out {2:N0} / read {3:N0} / write {4:N0})" -f $sum, $in, $out, $cr, $cw
@{ systemMessage = $msg } | ConvertTo-Json -Compress
exit 0
