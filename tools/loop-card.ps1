# Claude UserPromptSubmit hook: reminds every turn to open replies about AntiNotepad work with the status card
# (docs/loop.md, "Status card") and lists the items in flight from the live STATUS plan. Its output is added to Claude's
# context. Reads STATUS from the main checkout even when run from a worktree (a branch copy of STATUS is stale).
param([string]$Root = (Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference = 'Stop'
try { [void][Console]::In.ReadToEnd() } catch { }
try {
    $common = (& git -C $Root rev-parse --path-format=absolute --git-common-dir 2>$null)
    if ($LASTEXITCODE -eq 0 -and $common) { $Root = Split-Path $common.Trim() -Parent }
} catch { }
$status = Join-Path $Root '.coord/STATUS.md'

$live = @()
if (Test-Path $status) {
    $inPlan = $false
    foreach ($l in Get-Content $status -Encoding utf8) {
        if ($l -match '^## Plan') { $inPlan = $true; continue }
        if ($inPlan -and $l -match '^## ') { break }
        if (-not $inPlan -or $l -notmatch '^\|' -or $l -match '^\|[\s|:-]+\|\s*$') { continue }
        $c = $l.Trim().Trim('|').Split('|') | ForEach-Object { $_.Trim() }
        if ($c.Count -lt 5 -or $c[0] -eq 'Item' -or $c[0] -match '^\*\*' -or $c[0] -match '^Release marker') { continue }
        if ($c[4] -match '^(done|dropped|planned|needs you|proposed|-)?$') { continue }
        $live += "$($c[0]) ($($c[4]), $($c[2]))"
    }
}

# ASCII only: hook output passes through the console code page.
"AntiNotepad loop: if this reply is about an item or project work, open it with the status card from docs/loop.md (""Status card""): '>> ID-slug', tier and current phase; the phase strip Request | Findings | Plan | Go | Build | Check | Hand test | Merge with each phase marked done, current or not reached; Tier with its reason slug; Goal; Now; then a divider and Waiting on you."
if ($live.Count) { "In flight per STATUS: $($live -join '; ')." } else { 'In flight per STATUS: none.' }
exit 0
