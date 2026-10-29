<#
Loop v2.2 (DORMANT, not wired into any skill; see docs/v2.2/README.md).
Merges the latest dev into feat/<ID-slug> inside its worktree (never a rebase) and reports the result.
Prints UP TO DATE, CLEAN or CONFLICT (conflicted files, hot files marked). Exit 0 = up to date/clean, 2 = conflict, 1 = refused.
After CLEAN the caller re-runs tools/gate.ps1; after CONFLICT it triages (docs/v2.2/README.md). Does not push.
Usage: pwsh -NoProfile -File tools/v2.2/catch-up.ps1 -Task <ID-slug> [-Base dev]
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Task,
    [string]$Base = 'dev',
    [string[]]$HotFiles = @(
        'product/AntiNotepad/SelfTest/Contracts.cs',
        'product/AntiNotepad/SelfTest.cs',
        'docs/loop.md'
    )
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (& git -C $PSScriptRoot rev-parse --show-toplevel).Trim()
$branch = (& git -C $repoRoot rev-parse --abbrev-ref HEAD).Trim()
if ($branch -cne "feat/$Task") { Write-Host "REFUSED: checkout is on '$branch', expected feat/$Task."; exit 1 }
if (& git -C $repoRoot status --porcelain) { Write-Host 'REFUSED: worktree is not clean; commit or stash your own changes first.'; exit 1 }

& git -C $repoRoot fetch --quiet origin
$baseRef = if (& git -C $repoRoot rev-parse --verify --quiet "origin/$Base") { "origin/$Base" } else { $Base }
& git -C $repoRoot merge-base --is-ancestor $baseRef HEAD
if ($LASTEXITCODE -eq 0) { Write-Host "UP TO DATE with $baseRef"; exit 0 }

$incoming = @(& git -C $repoRoot diff --name-only "HEAD...$baseRef")
& git -C $repoRoot merge --no-ff -m "${Task}: catch up with dev" $baseRef | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "CLEAN: merged $baseRef ($($incoming.Count) incoming file(s)); re-run tools/gate.ps1 -Task $Task"
    $incoming | ForEach-Object { Write-Host "  incoming $_" }
    exit 0
}
$conflicts = @(& git -C $repoRoot diff --name-only --diff-filter=U)
Write-Host "CONFLICT: $($conflicts.Count) file(s); resolve on the branch (merge is left open)"
foreach ($c in $conflicts) { Write-Host ("  {0}{1}" -f $(if ($HotFiles -contains $c) { '[hot] ' } else { '' }), $c) }
exit 2
