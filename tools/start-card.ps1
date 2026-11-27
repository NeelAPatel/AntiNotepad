param([switch]$SelfTest)
$ErrorActionPreference = 'Stop'
function Get-StartCard([string]$Cwd, [string]$Root, [string]$Item = '') {
    $id = $Item
    if ($Cwd -match '(?i)[\\/]\.claude[\\/]worktrees[\\/][^\\/]+[\\/]([^\\/]+)') { $id = $Matches[1] }
    $role = 'director'; $merge = 'Directors merge accepted items to dev.'
    if ($id) {
        $role = 'item'
        $card = Join-Path $Cwd ".coord/tasks/$id.md"
        if (-not (Test-Path -LiteralPath $card)) { $card = Join-Path $Root ".coord/tasks/$id.md" }
        if (Test-Path -LiteralPath $card) {
            $content = [IO.File]::ReadAllText($card)
            if ($content -match '(?im)^## Children\s*\r?\n\s*[-*]\s+`?[A-Z][A-Za-z0-9]*-') { $role = 'umbrella' }
        }
        foreach ($file in @('.coord/STATUS.md', '.coord/pending.md')) {
            $path = Join-Path $Root $file
            if (Test-Path -LiteralPath $path) {
                foreach ($line in [IO.File]::ReadLines($path)) {
                    $itemCell = '(?:&nbsp;)*\s*'
                    if ($line -match ('^\|\s*' + $itemCell + [regex]::Escape($id) + '\s*\|\s*child of\s+([^:|]+):')) {
                        $role = 'umbrella child'; $merge = "Rebase when ready; parent merges into $($Matches[1])."; break
                    }
                }
            }
        }
    }
    "START CARD: $role$(if ($id) { " ($id)" })"
    if ($role -eq 'director') { 'Title: use the director role/version naming rule in docs/loop-sessions.md.' }
    else { 'Title: set and update it with the item row and phase; follow title-hint.' }
    $merge
    'Codex implements; item chat briefs and checks. Codex checks are offline.'
    'Read: docs/loop.md, docs/playbook.md, docs/agents.md.'
}
if ($SelfTest) {
    try {
        $x = @(Get-StartCard 'C:/repo' 'C:/repo')
        if ($x.Count -ge 25 -or $x[0] -ne 'START CARD: director' -or $x[1] -ne 'Title: use the director role/version naming rule in docs/loop-sessions.md.') { throw 'director card failed' }
        $testRoot = Join-Path ([IO.Path]::GetTempPath()) ('start-card-' + [guid]::NewGuid().ToString('N'))
        try {
            [void](New-Item -ItemType Directory -Path (Join-Path $testRoot '.coord') -Force)
            [IO.File]::WriteAllText((Join-Path $testRoot '.coord/STATUS.md'), "| &nbsp;&nbsp;A39d-padview-split | Child of A39: scope | Medium | Small | approved |`n")
            $child = @(Get-StartCard 'C:/repo/.claude/worktrees/fix/A39d-padview-split' $testRoot)
            if ($child[0] -ne 'START CARD: umbrella child (A39d-padview-split)') { throw 'indented child role failed' }
        } finally { Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction SilentlyContinue }
        'START CARD SELFTEST PASS'; exit 0
    } catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 }
}
try {
    $hook = [Console]::In.ReadToEnd() | ConvertFrom-Json
    if (-not $hook.cwd -or -not (Test-Path -LiteralPath $hook.cwd -PathType Container)) { exit 0 }
    $root = Split-Path $PSScriptRoot -Parent
    $common = & git -C $root rev-parse --path-format=absolute --git-common-dir 2>$null
    if ($LASTEXITCODE -eq 0 -and $common) { $root = Split-Path $common.Trim() -Parent }
    $item = ''
    if ($hook.transcript_path -and (Test-Path -LiteralPath $hook.transcript_path -PathType Leaf)) {
        foreach ($line in [IO.File]::ReadLines([string]$hook.transcript_path)) {
            try { $entry = $line | ConvertFrom-Json } catch { continue }
            if ($entry.type -eq 'user') {
                $content = if ($entry.message.content -is [string]) { $entry.message.content } else { (@($entry.message.content | Where-Object type -eq 'text' | ForEach-Object text) -join ' ') }
                if ($content -match '(?i)you are the item chat for\s+`?(?<id>[A-Z][A-Za-z0-9]*(?:-[a-z0-9]+)+)') { $item = $Matches.id; break }
            }
        }
    }
    Get-StartCard ([string]$hook.cwd) $root $item
} catch { }
exit 0
