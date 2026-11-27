[CmdletBinding()]
param([switch]$Serve, [ValidateRange(1,65535)][int]$Port = 47890)
$ErrorActionPreference = 'Stop'
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('umbrella-mock-' + [guid]::NewGuid().ToString('N'))
$passed = 0; $total = 0
$shell = (Get-Process -Id $PID).Path
$tools = $PSScriptRoot
$oldProject = $env:CLAUDE_PROJECT_DIR; $oldLoop = $env:ANTINOTEPAD_LOOP
function Check([string]$Name, [scriptblock]$Test) {
    $script:total++
    try { & $Test; $script:passed++; Write-Output "PASS ${Name}: ok" }
    catch { Write-Output "FAIL ${Name}: $($_.Exception.Message)" }
}
function Require([bool]$Condition, [string]$Why) { if (-not $Condition) { throw $Why } }
function Git([string[]]$Arguments) {
    $output = @(& git.exe -C $fixture @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) { throw "git $($Arguments -join ' '): $($output -join ' ')" }
    return $output
}
function Hook([string]$Script, [string]$Cwd, [string]$Transcript = '') {
    $json = @{ cwd = $Cwd; transcript_path = $Transcript } | ConvertTo-Json -Compress
    if ($Script -eq (Join-Path $tools 'title-hint.ps1')) { return @($json | & $shell -NoProfile -File $Script -Root $fixture) }
    return @($json | & $shell -NoProfile -File $Script)
}
function Brief([string]$Id) {
    $prompt = '/pstack:poteto-mode' + "`nYour first action is a Skill tool call`nYou are the item chat for $Id"
    $json = @{tool_input = @{prompt = $prompt}} | ConvertTo-Json -Compress
    $result = @($json | & $shell -NoProfile -File (Join-Path $tools 'poteto-gate.ps1') -Mode Brief 2>&1)
    return @{code = $LASTEXITCODE; output = ($result -join ' ')}
}
try {
    New-Item -ItemType Directory -Force -Path $fixture, (Join-Path $fixture '.coord/tasks'), (Join-Path $fixture 'tools') | Out-Null
    Copy-Item (Join-Path $tools 'start-card.ps1'), (Join-Path $tools 'merge-preflight.ps1') (Join-Path $fixture 'tools')
    Git @('init', '-q', '-b', 'dev') | Out-Null
    Git @('config', 'user.name', 'Mock') | Out-Null
    Git @('config', 'user.email', 'mock@example.invalid') | Out-Null
    Git @('config', 'core.autocrlf', 'false') | Out-Null
    @'
## Plan
### Pending work
| Item | Scope | Tier | Effort | State |
| --- | --- | --- | --- | --- |
| A90-mock-umbrella | umbrella | Medium | Small | building |
| A90a-mock-child | child of A90-mock-umbrella: plain | Medium | Small | approved |
| &nbsp;&nbsp;A90b-mock-child | Child of A90: indented | Medium | Small | approved |
'@ | Set-Content -LiteralPath (Join-Path $fixture '.coord/STATUS.md')
    "## Children`n- A90a-mock-child`n- A90b-mock-child`n" | Set-Content -LiteralPath (Join-Path $fixture '.coord/tasks/A90-mock-umbrella.md')
    "## Request`nFilled`n" | Set-Content -LiteralPath (Join-Path $fixture '.coord/tasks/A90z-missing-row.md')
    Git @('add', '.') | Out-Null
    Git @('commit', '-qm', 'fixture') | Out-Null
    $env:CLAUDE_PROJECT_DIR = $fixture
    $env:ANTINOTEPAD_LOOP = '1'
    $item = Join-Path $fixture '.claude/worktrees/chore/A90z-missing-row'
    $child = Join-Path $fixture '.claude/worktrees/chore/A90b-mock-child'
    New-Item -ItemType Directory -Force -Path $item, $child | Out-Null
    $start = Join-Path $fixture 'tools/start-card.ps1'
    Check 'start item' { Require ((Hook $start $item) -contains 'START CARD: item (A90z-missing-row)') 'item role missing' }
    Check 'start child' { Require ((Hook $start $child) -contains 'START CARD: umbrella child (A90b-mock-child)') 'child role missing' }
    Check 'start director' { Require ((Hook $start $fixture) -contains 'START CARD: director') 'director role missing' }
    $title = Join-Path $tools 'title-hint.ps1'
    Check 'title missing row' { Require (((Hook $title $item) -join "`n") -match 'title-hint A90z-missing-row: NO ROW \(card phase=') 'missing-row hint absent' }
    Check 'title child' { Require (((Hook $title $child) -join "`n") -match 'title-hint A90b-mock-child:') 'child hint absent' }
    $transcript = Join-Path $fixture 'transcript.jsonl'
    '{"type":"custom-title","customTitle":"wrong"}' | Set-Content -LiteralPath $transcript
    Check 'title differs' { Require (((Hook $title $child $transcript) -join "`n") -match 'title-hint WARN: real title differs') 'title warning absent' }
    Check 'brief missing row' { $r = Brief 'A90z-missing-row'; Require ($r.code -eq 2 -and $r.output -match 'A90z-missing-row') "expected exit 2 and missing ID, got $($r.code): $($r.output)" }
    Check 'brief plain row' { $r = Brief 'A90a-mock-child'; Require ($r.code -eq 0) "exit $($r.code): $($r.output)" }
    Check 'brief indented row' { $r = Brief 'A90b-mock-child'; Require ($r.code -eq 0) "exit $($r.code): $($r.output)" }
    Check 'roadmap snapshot' {
        $snapshot = Join-Path $fixture 'snapshot.html'
        & $shell -NoProfile -File (Join-Path $tools 'roadmap-data.ps1') -Root $fixture -Snapshot $snapshot | Out-Null
        Require ($LASTEXITCODE -eq 0) 'snapshot command failed'
        $html = [IO.File]::ReadAllText($snapshot)
        $match = [regex]::Match($html, 'window\.ROADMAP_DATA = (.*?);</script>')
        Require $match.Success 'snapshot data missing'
        $data = $match.Groups[1].Value | ConvertFrom-Json
        foreach ($id in @('A90a-mock-child', 'A90b-mock-child')) {
            $row = @($data.rows | Where-Object item -eq $id)
            Require ($row.Count -eq 1 -and $row[0].parent -eq 'A90-mock-umbrella') "$id parent missing"
        }
    }
    Git @('checkout', '-qb', 'chore/A90-mock-umbrella') | Out-Null
    $worktree = Join-Path $fixture '.claude/worktrees/chore/A90a-mock-child'
    Git @('worktree', 'add', '-qb', 'chore/A90a-mock-child', $worktree) | Out-Null
    @'
## Request
Filled
## Plan
Filled
## Approve
Filled
## Check
Filled
## Hand test
Filled
'@ | Set-Content -LiteralPath (Join-Path $worktree '.coord/tasks/A90a-mock-child.md')
    & git.exe -C $worktree add .coord/tasks/A90a-mock-child.md
    & git.exe -C $worktree commit -qm child
    if ($LASTEXITCODE -ne 0) { throw 'child commit failed' }
    Git @('commit', '--allow-empty', '-qm', 'advance umbrella') | Out-Null
    $preflight = Join-Path $fixture 'tools/merge-preflight.ps1'
    Check 'preflight behind' {
        $out = @(& $shell -NoProfile -File $preflight -Task A90a-mock-child -Base chore/A90-mock-umbrella -SkipBuilds)
        Require ($LASTEXITCODE -eq 1 -and ($out -join ' ') -match 'behind chore/A90-mock-umbrella') "expected behind failure: $out"
    }
    & git.exe -C $worktree rebase chore/A90-mock-umbrella 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'fixture rebase failed' }
    $head = (& git.exe -C $worktree rev-parse HEAD).Trim()
    New-Item -ItemType Directory -Force (Join-Path $worktree '.coord/reports') | Out-Null
    "Head: $head`nGATE: PASS`n" | Set-Content -LiteralPath (Join-Path $worktree '.coord/reports/A90a-mock-child.gate.md')
    Check 'preflight rebased' {
        $out = @(& $shell -NoProfile -File $preflight -Task A90a-mock-child -Base chore/A90-mock-umbrella -SkipBuilds)
        Require ($LASTEXITCODE -eq 0 -and ($out -join ' ') -match 'MERGE-PREFLIGHT: PASS') "expected pass: $out"
    }
} catch { $total++; Write-Output "FAIL fixture: $($_.Exception.Message)" }
finally {
    $env:CLAUDE_PROJECT_DIR = $oldProject; $env:ANTINOTEPAD_LOOP = $oldLoop
    if ($Serve) { Write-Output ("SERVE: pwsh -NoProfile -File `"{0}`" -Root `"{1}`" -Port {2}" -f (Join-Path $tools 'roadmap-serve.ps1'), $fixture, $Port) }
    elseif (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture -Recurse -Force -ErrorAction SilentlyContinue }
}
$result = if ($passed -eq $total) { 'PASS' } else { 'FAIL' }
Write-Output "UMBRELLA MOCK: $result ($passed/$total)"
if ($result -eq 'FAIL') { exit 1 }
