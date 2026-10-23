# Auto-resume watchdog for the claude/product-spike run (2026-10-22).
# Every 10 min: if the session transcript has been idle > 25 min and PROGRESS.md is not COMPLETE,
# resume the session headless. Stops on COMPLETE, on spike-notes/STOP, or after 16 hours.
$ErrorActionPreference = 'Continue'
$repo = 'F:\LocalVSProj\Visual Studio Apps\AntiQuickPad'
$dir = "$env:USERPROFILE\.claude\projects\F--LocalVSProj-Visual-Studio-Apps-AntiQuickPad"
$session = '966c1dc8-0742-4d57-af39-7b41c8396fd3'
$progress = "$repo\spike-notes\PROGRESS.md"
$log = "$repo\spike-notes\watchdog.log"
$deadline = (Get-Date).AddHours(16)
function Log($m) { Add-Content $log "$(Get-Date -Format s) $m" }
Log "watchdog start pid=$PID session=$session"
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 600
    if (Test-Path "$repo\spike-notes\STOP") { Log 'STOP file; exit'; break }
    if ((Test-Path $progress) -and (Select-String -Path $progress -Pattern 'STATUS: COMPLETE' -Quiet)) { Log 'COMPLETE; exit'; break }
    # Idle = newest transcript in the project dir (covers a resumed run writing a new session file).
    $newest = Get-ChildItem $dir -Filter *.jsonl | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    $idle = ((Get-Date) - $newest.LastWriteTime).TotalMinutes
    if ($idle -lt 25) { continue }
    Log "idle=$([int]$idle)m; resuming $session"
    Set-Location $repo
    $out = & claude -p --resume $session --permission-mode auto --model opus --output-format json `
        "Watchdog resume (usage limit likely reset). Re-read spike-notes/PROGRESS.md and git log on claude/product-spike, then continue the Antipad spike work autonomously. The user is asleep; do not ask questions. Keep committing per feature and updating PROGRESS.md. When everything is done, write 'STATUS: COMPLETE' in PROGRESS.md." 2>&1 | Out-String
    try { $j = $out | ConvertFrom-Json; if ($j.session_id) { $session = $j.session_id }; Log "run done session=$session is_error=$($j.is_error) result=$(([string]$j.result).Substring(0, [Math]::Min(200, ([string]$j.result).Length)))" }
    catch { Log "run exit=$LASTEXITCODE raw: $(($out.Trim() -split "`n" | Select-Object -Last 3) -join ' | ')" }
}
Log 'watchdog end'
