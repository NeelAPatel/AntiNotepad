# Claude SessionStart hook: shows the current item's phase and next action.
# Opt-in: part of my dev loop. Set ANTINOTEPAD_LOOP=1 to enable (see docs/claude-hooks.md).
if ($env:ANTINOTEPAD_LOOP -cne '1') { exit 0 }
$ErrorActionPreference = 'Stop'
try {
    $root = [string]$env:CLAUDE_PROJECT_DIR
    if (-not $root) { $root = Split-Path $PSScriptRoot -Parent }
    $branch = [string](& git -C $root rev-parse --abbrev-ref HEAD 2>$null)
    if ($LASTEXITCODE -ne 0 -or $branch -notmatch '^[^/]+/(.+)$') { exit 0 }
    $slug = $Matches[1]
    $cardRel = ".coord/tasks/$slug.md"
    $card = Join-Path $root $cardRel
    if (-not (Test-Path -LiteralPath $card)) {
        $common = [string](& git -C $root rev-parse --path-format=absolute --git-common-dir 2>$null)
        if ($LASTEXITCODE -eq 0) { $card = Join-Path (Split-Path $common -Parent) $cardRel }
    }
    if (-not (Test-Path -LiteralPath $card)) { exit 0 }

    $headings = @('Request', 'Findings', 'Plan', 'Approve', 'Build', 'Check', 'Hand test', 'Merge')
    $bodies = @{}
    $active = ''
    foreach ($line in (Get-Content $card -Encoding utf8)) {
        if ($line -match '^## (.+?)\s*$') {
            $active = $Matches[1]
            if ($active -notin $headings) { $active = '' }
            elseif (-not $bodies.ContainsKey($active)) { $bodies[$active] = '' }
            continue
        }
        if ($active -and $line.Trim()) { $bodies[$active] += $line.Trim() }
    }
    $phase = ''
    foreach ($heading in $headings) { if ($bodies[$heading]) { $phase = $heading } }
    if (-not $phase) { exit 0 }
    $nextByPhase = @{
        Request='write the Plan'; Findings='write the Plan'; Plan='wait for user approval'; Approve='build with Codex'
        Build='run the gate and Check'; Check='hand test'; 'Hand test'='wait for merge'; Merge='done'
    }
    $status = Join-Path $root '.coord/STATUS.md'
    $state = ''
    if (Test-Path -LiteralPath $status) {
        foreach ($line in (Get-Content $status -Encoding utf8)) {
            if ($line -match ('^\|\s*' + [regex]::Escape($slug) + '\s*\|')) {
                $cells = $line.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
                if ($cells.Count -ge 5) { $state = $cells[4] }
                break
            }
        }
    }
    "Item: $slug"
    "Phase: $phase$(if ($state) { " (STATUS: $state)" })"
    "Next: $($nextByPhase[$phase])"
    "Card: $cardRel"
}
catch { }
exit 0
