param(
    [string]$Item,
    [ValidateSet('blocked','needs-you','waiting','working','parked','standby')][string]$State = 'working',
    [switch]$Codex, [string]$Phase, [switch]$Merged, [switch]$Closed, [string]$Name, [string]$SessionId, [switch]$Sync, [switch]$Check
)
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)
$utf8 = [Text.UTF8Encoding]::new($false)
$root = Split-Path $PSScriptRoot -Parent
function Set-ComputedTitle([string]$Store, [string]$Key, [string]$Title) {
    [IO.Directory]::CreateDirectory($Store) | Out-Null
    $path = Join-Path $Store "$Key.txt"
    if ((Test-Path -LiteralPath $path) -and [IO.File]::ReadAllText($path, $utf8) -ceq $Title) { return }
    [IO.File]::WriteAllText($path, $Title, $utf8)
    "SET TITLE: $Title"
}
function Get-Title([string]$ItemName, [string]$TitleState, [bool]$HasCodex, [string]$TitlePhase) {
    $glyphs = @{ blocked='⛔'; 'needs-you'='❓'; waiting='⏳'; working='🔨'; parked='🅿️'; standby='💤' }
    $badge = if ($HasCodex) { '🤖' } else { '' }
    "$($glyphs[$TitleState])$badge $ItemName · $TitlePhase"
}
function Test-SessionId([string]$Id) { return $Id -cmatch '^[A-Za-z0-9][A-Za-z0-9_-]{0,127}$' }
function Set-SessionItem([string]$Main, [string]$Id, [string]$ItemName) {
    if (-not (Test-SessionId $Id)) { return $false }
    $dir = Join-Path $Main '.coord/titles/sessions'
    [IO.Directory]::CreateDirectory($dir) | Out-Null
    [IO.File]::WriteAllText((Join-Path $dir "$Id.txt"), $ItemName, $utf8)
    return $true
}
function Get-SessionItem([string]$Main, [string]$Id) {
    if (-not (Test-SessionId $Id)) { return '' }
    $path = Join-Path $Main ".coord/titles/sessions/$Id.txt"
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        $value = [IO.File]::ReadAllText($path, $utf8).Trim()
        if ($value -cmatch '^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') { return $value }
    }
    return ''
}
function Get-StatusRows([string]$Main) {
    $path = Join-Path $Main '.coord/STATUS.md'
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return }
    foreach ($line in Get-Content -LiteralPath $path -Encoding utf8) {
        if ($line -notmatch '^\|') { continue }
        $cells = @($line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() })
        if ($cells.Count -ge 5 -and $cells[0] -cmatch '^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') {
            [pscustomobject]@{ Item = $cells[0]; State = $cells[4] }
        }
    }
}
function Get-PhaseValue([string]$Main, [string]$ItemName, [string[]]$Roots) {
    $row = @(Get-StatusRows $Main | Where-Object { $_.Item -ceq $ItemName } | Select-Object -First 1)
    if ($row.Count) { return $row[0].State }
    foreach ($dir in @($Main) + $Roots) {
        $card = Join-Path $dir ".coord/tasks/$ItemName.md"
        if (Test-Path -LiteralPath $card -PathType Leaf) {
            $line = Get-Content -LiteralPath $card -Encoding utf8 | Where-Object { $_ -match '^State:\s*' } | Select-Object -First 1
            if ($line) { return ($line -replace '^State:\s*', '') }
        }
    }
    return ''
}
function ConvertTo-Phase([string]$Value, [string]$Round) {
    $phase = switch -Regex ($Value) {
        '(?i)hand[ -]?test|accepted' { 'hand test'; break }
        '(?i)check|review' { 'check'; break }
        '(?i)approv|build|rework' { 'build'; break }
        '(?i)plan' { 'plan'; break }
        '(?i)findings|audit' { 'findings'; break }
        default { 'request' }
    }
    if ($Value -match '(?i)\br((?:[2-9]|[1-9][0-9]+))\b') { $phase += " · r$($Matches[1])" }
    elseif ($Round) { $phase += " · $Round" }
    return $phase
}
function Sync-Sessions([string]$Main, [string[]]$Roots, [string]$DataRoot) {
    $items = @{}
    foreach ($row in @(Get-StatusRows $Main)) { $items[$row.Item] = $true }
    foreach ($dir in @($Main) + $Roots) {
        $cards = Join-Path $dir '.coord/tasks'
        if (Test-Path -LiteralPath $cards -PathType Container) {
            foreach ($file in Get-ChildItem -LiteralPath $cards -Filter '*.md' -File) {
                if ($file.BaseName -cmatch '^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') { $items[$file.BaseName] = $true }
            }
        }
    }
    if (-not (Test-Path -LiteralPath $DataRoot -PathType Container)) { return }
    foreach ($file in Get-ChildItem -LiteralPath $DataRoot -Filter 'local_*.json' -File -Recurse -ErrorAction SilentlyContinue) {
        try { $chat = Get-Content -LiteralPath $file.FullName -Raw -Encoding utf8 | ConvertFrom-Json -ErrorAction Stop } catch { continue }
        $titleText = [string]$chat.title
        $found = @($items.Keys | Where-Object { $titleText -cmatch ('(?<![A-Za-z0-9-])' + [regex]::Escape($_) + '(?![A-Za-z0-9-])') })
        if ($found.Count -eq 0) {
            $found = @($items.Keys | Where-Object {
                $id = ($_ -split '-')[0]
                $titleText -cmatch ('(?<![A-Za-z0-9-])' + [regex]::Escape($id) + '(?![A-Za-z0-9-])')
            })
        }
        if ($found.Count -eq 1 -and (Set-SessionItem $Main ([string]$chat.cliSessionId) $found[0])) { "REGISTERED: $($chat.cliSessionId) $($found[0])" }
    }
}
if ($Check) {
    $temp = Join-Path ([IO.Path]::GetTempPath()) ('chat-title-' + [guid]::NewGuid().ToString('N'))
    try {
        $expected = @{ blocked='⛔'; 'needs-you'='❓'; waiting='⏳'; working='🔨'; parked='🅿️'; standby='💤' }
        foreach ($s in $expected.Keys) {
            $title = Get-Title 'T1-example' $s $false 'build'
            if ($title -cne "$($expected[$s]) T1-example · build") { throw "state $s" }
            $line = Set-ComputedTitle $temp 'T1-example' $title
            if ($line -cne "SET TITLE: $title") { throw "store $s" }
            if (Set-ComputedTitle $temp 'T1-example' $title) { throw 'duplicate output' }
        }
        $title = Get-Title 'T1-example' 'waiting' $true 'build'
        if ($title -cne '⏳🤖 T1-example · build') { throw 'badge' }
        foreach ($title in @($title, '[Merged] T1-example', '[Closed] director')) {
            if ((Set-ComputedTitle $temp 'T1-example' $title) -cne "SET TITLE: $title") { throw 'form' }
        }
        $bytes = $utf8.GetBytes('⏳🤖 🅿️')
        if ($utf8.GetString($bytes) -cne '⏳🤖 🅿️' -or ($bytes[0] -eq 0xEF)) { throw 'UTF-8' }
        $fixture = Join-Path $temp 'fixture'
        $worktree = Join-Path $temp 'worktree'
        [IO.Directory]::CreateDirectory((Join-Path $fixture '.coord/tasks')) | Out-Null
        [IO.Directory]::CreateDirectory((Join-Path $worktree '.coord/tasks')) | Out-Null
        [IO.File]::WriteAllText((Join-Path $fixture '.coord/STATUS.md'), "| Item | Scope | Tier | Effort | State |`n| A22-wrap-hanging-indent | x | x | x | hand test |`n| A23-paste-keep-structure | x | x | x | rework r4 |`n| T9-one | x | x | x | building |`n| T9-two | x | x | x | checking |", $utf8)
        [IO.File]::WriteAllText((Join-Path $worktree '.coord/tasks/A22-wrap-hanging-indent.md'), 'State: building', $utf8)
        [IO.File]::WriteAllText((Join-Path $worktree '.coord/tasks/A24-fallback.md'), 'State: checking', $utf8)
        $cases = @(
            @('STATUS wins', 'A22-wrap-hanging-indent', 'hand test', '🔨 A22-wrap-hanging-indent · hand test'),
            @('STATUS rework', 'A23-paste-keep-structure', 'build · r4', '🔨 A23-paste-keep-structure · build · r4'),
            @('worktree card', 'A24-fallback', 'check', '🔨 A24-fallback · check'),
            @('missing card', 'A25-missing', 'request', '🔨 A25-missing · request')
        )
        foreach ($case in $cases) {
            $phaseResult = ConvertTo-Phase (Get-PhaseValue $fixture $case[1] @($worktree)) ''
            $result = Get-Title $case[1] 'working' $false $phaseResult
            if ($phaseResult -cne $case[2] -or $result -cne $case[3]) { throw "case $($case[0]): $result" }
            "CASE: $($case[0]) | $($case[1]) | $result"
        }
        $roundName = 'A22-wrap-hanging-indent-r2'
        if ($roundName -cnotmatch '^(.*)-r([0-9]+)$') { throw 'round name' }
        $baseName = $Matches[1]; $round = 'r' + $Matches[2]
        $result = Get-Title $baseName 'working' $false (ConvertTo-Phase (Get-PhaseValue $fixture $baseName @($worktree)) $round)
        if ($result -cne '🔨 A22-wrap-hanging-indent · hand test · r2') { throw 'round title' }
        "CASE: round name | $roundName | $result"
        if (-not (Set-SessionItem $fixture 'session-123' 'A22-wrap-hanging-indent')) { throw 'registry write' }
        if ((Get-SessionItem $fixture 'session-123') -cne 'A22-wrap-hanging-indent') { throw 'registry lookup' }
        if ((Get-SessionItem $fixture '../bad') -or (Set-SessionItem $fixture '../bad' 'T1-example')) { throw 'bad session id' }
        "CASE: session registry | session-123 | $(Get-Title (Get-SessionItem $fixture 'session-123') 'working' $false 'hand test')"
        'CASE: bad session id | ../bad | no registry item'
        $data = Join-Path $temp 'claude-code-sessions/nested'
        [IO.Directory]::CreateDirectory($data) | Out-Null
        [IO.File]::WriteAllText((Join-Path $data 'local_1.json'), '{"title":"A22-wrap-hanging-indent","cliSessionId":"sync-1"}', $utf8)
        [IO.File]::WriteAllText((Join-Path $data 'local_2.json'), '{"title":"A23","cliSessionId":"sync-2"}', $utf8)
        [IO.File]::WriteAllText((Join-Path $data 'local_3.json'), '{"title":"T9","cliSessionId":"sync-3"}', $utf8)
        [IO.File]::WriteAllText((Join-Path $data 'local_4.json'), '{"title":"A22","cliSessionId":"../bad"}', $utf8)
        $before = @(Get-ChildItem -LiteralPath $data -File | ForEach-Object { [IO.File]::ReadAllText($_.FullName, $utf8) })
        $registrations = @(Sync-Sessions $fixture @($worktree) (Split-Path $data -Parent))
        if ($registrations.Count -ne 2 -or (Get-SessionItem $fixture 'sync-1') -cne 'A22-wrap-hanging-indent' -or (Get-SessionItem $fixture 'sync-2') -cne 'A23-paste-keep-structure' -or (Get-SessionItem $fixture 'sync-3')) { throw 'sync fixture' }
        $after = @(Get-ChildItem -LiteralPath $data -File | ForEach-Object { [IO.File]::ReadAllText($_.FullName, $utf8) })
        if (Compare-Object $before $after -SyncWindow 0) { throw 'sync modified app files' }
        'CASE: sync exact and unique ID | fixture titles A22-wrap-hanging-indent, A23 | registered both'
        'CASE: sync ambiguous ID | fixture title T9 | no registration'
        $bad = & pwsh -NoProfile -File $PSCommandPath -Item T1-example -State invalid 2>&1
        if ($LASTEXITCODE -eq 0) { throw 'bad state accepted' }
        'chat-title CHECK PASS'
    } finally { if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Recurse -Force } }
    exit 0
}
if ($Merged -and $Closed) { throw 'Choose -Merged or -Closed.' }
$common = [string](& git -C $root rev-parse --path-format=absolute --git-common-dir 2>$null)
if ($LASTEXITCODE -ne 0 -or -not $common) { throw 'Cannot resolve main checkout.' }
$main = Split-Path $common.Trim() -Parent
$roots = @(& git -C $root worktree list --porcelain | Where-Object { $_ -like 'worktree *' } | ForEach-Object { $_.Substring(9) })
if ($Sync) {
    $dataRoot = Join-Path $env:LOCALAPPDATA 'Claude-Data'
    foreach ($dir in @(Get-ChildItem -LiteralPath $dataRoot -Directory -Filter 'claude-code-sessions' -Recurse -ErrorAction SilentlyContinue)) { Sync-Sessions $main $roots $dir.FullName }
    exit 0
}
$explicitItem = [bool]$Item
if (-not $Item -and -not $Closed) {
    $lookupId = if ($SessionId) { $SessionId } else { $env:CLAUDE_CODE_SESSION_ID }
    $Item = Get-SessionItem $main $lookupId
    if (-not $Item) {
        $branch = [string](& git -C (Get-Location).Path rev-parse --abbrev-ref HEAD 2>$null)
        if ($LASTEXITCODE -eq 0 -and $branch -match '^[^/]+/([A-Z][A-Za-z0-9]*(-[a-z0-9]+)+)$') { $Item = $Matches[1] }
        else { exit 0 }
    }
}
$round = ''
if ($Item -and $Item -cmatch '^(.*)-r([0-9]+)$') { $Item = $Matches[1]; $round = 'r' + $Matches[2] }
if ($Item -and $Item -cnotmatch '^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$') { throw 'Invalid item name.' }
if ($explicitItem -and $env:CLAUDE_CODE_SESSION_ID) { [void](Set-SessionItem $main $env:CLAUDE_CODE_SESSION_ID $Item) }
if ($Closed -and [string]::IsNullOrWhiteSpace($Name)) { throw '-Closed requires -Name.' }
if ($Closed) {
    $title = "[Closed] $Name"
    if (-not $Item) { $Item = 'closed-' + [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($utf8.GetBytes($Name))) }
} elseif ($Merged) { $title = "[Merged] $Item" }
else {
    if (-not $Phase) {
        $value = Get-PhaseValue $main $Item $roots
        $Phase = ConvertTo-Phase $value $round
    }
    $title = Get-Title $Item $State ([bool]$Codex) $Phase
}
Set-ComputedTitle (Join-Path $main '.coord/titles') $Item $title
