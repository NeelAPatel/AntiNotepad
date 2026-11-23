param([string]$Root = (Split-Path $PSScriptRoot -Parent), [switch]$SelfTest)
# Opt-in: part of my dev loop. Set ANTINOTEPAD_LOOP=1 to enable (see docs/claude-hooks.md).
if ($env:ANTINOTEPAD_LOOP -cne '1' -and -not $SelfTest) { exit 0 }
$ErrorActionPreference = 'Stop'

function Get-CardPhase([string]$Base, [string]$Cwd, [string]$Id) {
    $relative = ".coord/tasks/$Id.md"
    $candidates = @()
    $worktrees = Join-Path $Base '.claude/worktrees'
    if (Test-Path -LiteralPath $worktrees -PathType Container) {
        foreach ($prefix in @(Get-ChildItem -LiteralPath $worktrees -Directory)) {
            $candidates += Join-Path (Join-Path $prefix.FullName $Id) $relative
        }
    }
    if ($Cwd -match '(?i)^(.+?[\\/]\.claude[\\/]worktrees[\\/][^\\/]+[\\/]([^\\/]+))(?:[\\/]|$)' -and $Matches[2] -ceq $Id) {
        $candidates += Join-Path $Matches[1] $relative
    }
    $candidates += Join-Path $Base $relative
    $headings = @('Request', 'Findings', 'Plan', 'Approve', 'Build', 'Check', 'Hand test', 'Merge')
    foreach ($card in $candidates) {
        if (-not (Test-Path -LiteralPath $card -PathType Leaf)) { continue }
        $bodies = @{}; $active = ''
        foreach ($line in [IO.File]::ReadAllLines($card)) {
            if ($line -match '^## (.+?)\s*$') {
                $active = $Matches[1]
                if ($active -notin $headings) { $active = '' }
                elseif (-not $bodies.ContainsKey($active)) { $bodies[$active] = '' }
            } elseif ($active -and $line.Trim()) { $bodies[$active] += $line.Trim() }
        }
        $last = ''
        foreach ($heading in $headings) { if ($bodies[$heading]) { $last = $heading } }
        switch ($last) {
            { $_ -in @('Request', 'Findings') } { return 'findings' }
            'Plan' { return 'plan' }
            { $_ -in @('Approve', 'Build') } { return 'build' }
            'Check' { return 'check' }
            'Hand test' { return 'hand-test' }
            default { return 'none' }
        }
    }
    return 'none'
}

function Get-Hints([string]$Base, [string]$Cwd) {
    $rows = @{}
    foreach ($file in @('.coord/pending.md', '.coord/STATUS.md')) {
        $path = Join-Path $Base $file
        if (-not (Test-Path -LiteralPath $path)) { continue }
        $inPlan = $file -ne '.coord/STATUS.md'
        foreach ($line in [IO.File]::ReadAllLines($path)) {
            if ($file -eq '.coord/STATUS.md' -and $line -match '^## Plan') { $inPlan = $true; continue }
            if ($file -eq '.coord/STATUS.md' -and $inPlan -and $line -match '^## ') { break }
            if (-not $inPlan -or $line -notmatch '^\|') { continue }
            $cells = @($line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() })
            if ($cells.Count -lt 5 -or $cells[0] -cnotmatch '^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') { continue }
            $state = $cells[4].ToLowerInvariant()
            if ($file -eq '.coord/pending.md' -and $state -notmatch '^(request|findings|plan ready|needs you|paused|parked)') { continue }
            $rows[$cells[0]] = $state
        }
    }
    $item = $null
    if ($Cwd -match '(?i)[\\/]\.claude[\\/]worktrees[\\/][^\\/]+[\\/]([^\\/]+)(?:[\\/]|$)') { $item = $Matches[1] }
    $logs = Join-Path $Base '.coord/logs'
    $ledger = Join-Path $logs 'usage.tsv'
    foreach ($id in @($rows.Keys | Sort-Object)) {
        if ($item -and $id -cne $item) { continue }
        $state = $rows[$id]
        $phase = 'none'; $dev = 'none'; $codex = 'none'; $me = 'none'
        if ($state -match '\b(paused|parked|merged|dropped)\b') { $dev = $Matches[1] }
        if ($state -match '\bdone\b') { $dev = 'merged' }
        if ($state -match 'hand[ -]tested|hand test') { $phase = 'hand-test' }
        elseif ($state -match 'ask-user|checking|accepted') { $phase = 'check' }
        elseif ($state -match 'approved|building|rework') { $phase = 'build' }
        elseif ($state -match 'plan ready') { $phase = 'plan' }
        elseif ($state -match 'findings') { $phase = 'findings' }
        if ($state -match '^(paused|parked)\s*$') { $phase = Get-CardPhase $Base $Cwd $id }
        if ($state -match '\bdone\b|\bmerged\b|\bdropped\b') { $phase = 'none' }
        if ($state -match 'plan ready|hand test|ask-user|needs you') { $me = 'needs-you' }
        $taskPattern = '^' + [regex]::Escape($id) + '(?:-r[0-9]+)?(?:-audit|-review)?$'
        $lastTime = [datetime]::MinValue; $lastExit = $null
        if (Test-Path -LiteralPath $ledger) {
            foreach ($entry in [IO.File]::ReadLines($ledger)) {
                $parts = $entry -split "`t"
                $when = [datetime]::MinValue
                if ($parts.Count -ge 6 -and $parts[4] -cmatch $taskPattern -and $parts[5] -match '^exit=(\d+)$' -and [datetime]::TryParse($parts[1], [ref]$when)) {
                    if ($when -ge $lastTime) { $lastTime = $when; $lastExit = [int]$Matches[1] }
                }
            }
        }
        $latest = $null
        if (Test-Path -LiteralPath $logs) {
            $latest = Get-ChildItem -LiteralPath $logs -File | Where-Object { $_.Name -cmatch ($taskPattern.TrimEnd('$') + '\.[^.]+\.log$') } | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        }
        # Ledger timestamps have second precision; compare at that precision.
        if ($latest -and $latest.LastWriteTime -ge $lastTime.AddSeconds(1)) {
            if ($latest.LastWriteTime -lt (Get-Date).AddMinutes(-20)) { $codex = 'stalled' } else { $codex = 'codexing' }
        } elseif ($null -ne $lastExit -and $lastExit -ne 0) { $codex = 'failed' }
        if ($codex -in @('failed', 'stalled')) { $me = 'needs-you' }
        if ($state -match 'rework') { $me = 'none' }
        "title-hint ${id}: me=$me codex=$codex dev=$dev phase=$phase"
    }
}

function Invoke-SelfTest {
    $base = Join-Path ([IO.Path]::GetTempPath()) ('title-hint-' + [guid]::NewGuid().ToString('N'))
    try {
        $logDir = Join-Path $base '.coord/logs'; [void](New-Item -ItemType Directory -Path $logDir -Force)
        $status = Join-Path $base '.coord/STATUS.md'; $ledger = Join-Path $logDir 'usage.tsv'
        $id = 'A28-emoji-render-gaps'; $log = Join-Path $logDir "$id.terra.log"
        function Row([string]$state) { [IO.File]::WriteAllText($status, "## Plan`n| $id | scope | Medium | Small | $state |`n## Open`n") }
        function Check([string]$name, [string]$expected) {
            $actual = @(Get-Hints $base $base) -join "`n"
            if ($actual -cne "title-hint ${id}: $expected") { throw "$name : $actual" }
        }
        Row 'hand test'; Check 'hand test' 'me=needs-you codex=none dev=none phase=hand-test'
        Row 'building'; [IO.File]::WriteAllText($log, 'run'); Check 'running' 'me=none codex=codexing dev=none phase=build'
        (Get-Item $log).LastWriteTime = (Get-Date).AddMinutes(-21); Check 'stalled' 'me=needs-you codex=stalled dev=none phase=build'
        [IO.File]::WriteAllText($ledger, "tag`t$((Get-Date).ToString('s'))`tterra`tmodel`t$id`texit=1`t1s`n")
        Check 'failed' 'me=needs-you codex=failed dev=none phase=build'
        Row 'rework r2'; Check 'rework' 'me=none codex=failed dev=none phase=build'
        [IO.File]::WriteAllText($ledger, "tag`t$((Get-Date).ToString('s'))`tterra`tmodel`t$id`texit=0`t1s`n")
        Row 'checking'; Check 'check' 'me=none codex=none dev=none phase=check'
        Row 'paused building'; Check 'paused resume' 'me=none codex=none dev=paused phase=build'
        $reviewLog = Join-Path $logDir "$id-r2-review.reviewer.log"
        [IO.File]::WriteAllText($reviewLog, 'review'); (Get-Item $reviewLog).LastWriteTime = (Get-Date).AddSeconds(2)
        Row 'checking'; Check 'review running' 'me=none codex=codexing dev=none phase=check'
        [IO.File]::WriteAllText($ledger, "tag`t$((Get-Date).AddSeconds(4).ToString('s'))`treviewer`tmodel`t$id-r2-review`texit=1`t1s`n")
        Check 'review failed' 'me=needs-you codex=failed dev=none phase=check'
        [IO.File]::WriteAllText($ledger, "tag`t$((Get-Date).AddSeconds(4).ToString('s'))`treviewer`tmodel`t$id-r2-review`texit=0`t1s`n")
        Row 'merged'; Check 'merged' 'me=none codex=none dev=merged phase=none'
        $cardDir = Join-Path $base '.coord/tasks'; [void](New-Item -ItemType Directory -Path $cardDir -Force)
        $card = Join-Path $cardDir "$id.md"
        [IO.File]::WriteAllText($card, "## Request`nRequest text`n## Plan`nPlan text`n## Check`nChecked`n## Hand test`n")
        Row 'paused'; Check 'status paused' 'me=none codex=none dev=paused phase=check'
        Row 'done'; Check 'done' 'me=none codex=none dev=merged phase=none'
        [IO.File]::WriteAllText($status, "## Plan`n## Open`n")
        $pending = Join-Path $base '.coord/pending.md'
        [IO.File]::WriteAllText($pending, "| $id | scope | Medium | Small | paused |`n")
        Check 'pending paused' 'me=none codex=none dev=paused phase=check'
        Remove-Item -LiteralPath $card
        Check 'missing card' 'me=none codex=none dev=paused phase=none'
        $workCard = Join-Path $base ".claude/worktrees/chore/$id/.coord/tasks/$id.md"
        [void](New-Item -ItemType Directory -Path (Split-Path $workCard) -Force)
        [IO.File]::WriteAllText($workCard, "## Build`nBuilt`n")
        [IO.File]::WriteAllText($pending, "| $id | scope | Medium | Small | parked |`n")
        Check 'worktree parked' 'me=none codex=none dev=parked phase=build'
        $shell = (Get-Process -Id $PID).Path
        $oldLoop = $env:ANTINOTEPAD_LOOP
        try {
            $env:ANTINOTEPAD_LOOP = '1'
            $garbage = '{' | & $shell -NoProfile -File $PSCommandPath -Root $base
        } finally { $env:ANTINOTEPAD_LOOP = $oldLoop }
        if ($LASTEXITCODE -ne 0 -or $garbage) { throw 'garbage did not fail open' }
        'TITLE HINT SELFTEST PASS (hand test, running, stalled, failed, rework, check, paused resume, review running, review failed, merged, status paused, done, pending paused, missing card, worktree parked, garbage)'
    } finally { Remove-Item -LiteralPath $base -Recurse -Force -ErrorAction SilentlyContinue }
}
if ($SelfTest) { try { Invoke-SelfTest; exit 0 } catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 } }
try {
    $hook = [Console]::In.ReadToEnd() | ConvertFrom-Json
    if (-not $hook.cwd -or -not (Test-Path -LiteralPath ([string]$hook.cwd) -PathType Container)) { exit 0 }
    if (-not $PSBoundParameters.ContainsKey('Root')) {
        $common = & git -C $Root rev-parse --path-format=absolute --git-common-dir 2>$null
        if ($LASTEXITCODE -eq 0 -and $common) { $Root = Split-Path $common.Trim() -Parent }
    }
    Get-Hints $Root ([string]$hook.cwd)
} catch { }
exit 0
