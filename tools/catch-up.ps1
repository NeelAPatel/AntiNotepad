<#
Loop v3.1: fetches and merges dev into an item checkout; never rebases or pushes. Re-run the gate after CLEAN.
Usage: pwsh -NoProfile -File tools/catch-up.ps1 -Task <ID-slug> [-Base dev]
Exit 0 = UP TO DATE/CLEAN; 1 = REFUSED; 2 = CONFLICT (hot files marked, merge left open for resolution).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Task,
    [string]$Base = 'dev',
    [string[]]$HotFiles = @(
        'product/AntiNotepad/SelfTest/Contracts.cs',
        'product/AntiNotepad/SelfTest.cs',
        'product/AntiNotepad/SelfTest/*.cs',
        'docs/loop.md',
        'docs/glossary.md',
        '.claude/skills/*/SKILL.md'
    )
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$prefixes = @('feat', 'fix', 'refactor', 'docs', 'chore')
$branchPattern = '^(' + ($prefixes -join '|') + ')/(?<slug>[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+)$'

$repoRoot = & git -C $PSScriptRoot rev-parse --show-toplevel
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot find repository root.'; exit 1 }
$repoRoot = $repoRoot.Trim()
$branch = & git -C $repoRoot rev-parse --abbrev-ref HEAD
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot determine current branch.'; exit 1 }
$branch = $branch.Trim()
if ($branch -cnotmatch $branchPattern -or $Matches.slug -cne $Task) {
    Write-Host "REFUSED: checkout is on '$branch', expected <prefix>/$Task ($($prefixes -join ', '))."; exit 1
}
$changes = @(& git -C $repoRoot status --porcelain --untracked-files=no)
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: git status failed.'; exit 1 }
if (@($changes | Where-Object { $_ -cnotmatch '^.. \.coord/STATUS\.md$' }).Count -gt 0) {
    Write-Host 'REFUSED: tracked changes outside .coord/STATUS.md; commit or stash your own changes first.'; exit 1
}

$fetchOutput = @(& git -C $repoRoot fetch --quiet origin 2>&1)
if ($LASTEXITCODE -ne 0) { Write-Host "REFUSED: $($fetchOutput -join [Environment]::NewLine)"; exit 1 }
$baseRef = "origin/$Base"
& git -C $repoRoot rev-parse --verify --quiet $baseRef > $null
if ($LASTEXITCODE -eq 1) { $baseRef = $Base }
elseif ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot resolve remote base.'; exit 1 }
& git -C $repoRoot rev-parse --verify --quiet $baseRef > $null
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot resolve base.'; exit 1 }
& git -C $repoRoot merge-base --is-ancestor $baseRef HEAD
if ($LASTEXITCODE -eq 0) { Write-Host "UP TO DATE with $baseRef"; exit 0 }
if ($LASTEXITCODE -ne 1) { Write-Host 'REFUSED: cannot compare base with HEAD.'; exit 1 }

$incoming = @(& git -C $repoRoot diff --name-only "HEAD...$baseRef")
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot list incoming files.'; exit 1 }
$mergeOutput = @(& git -C $repoRoot merge --no-ff -m "${Task}: catch up with dev" $baseRef 2>&1)
if ($LASTEXITCODE -eq 0) {
    Write-Host "CLEAN: merged $baseRef ($($incoming.Count) incoming file(s)); re-run tools/gate.ps1 -Task $Task"
    $incoming | ForEach-Object { Write-Host "  incoming $_" }
    exit 0
}
$conflicts = @(& git -C $repoRoot diff --name-only --diff-filter=U)
if ($LASTEXITCODE -ne 0) { Write-Host 'REFUSED: cannot list conflicts.'; exit 1 }
if ($conflicts.Count -eq 0) { Write-Host "REFUSED: $($mergeOutput -join [Environment]::NewLine)"; exit 1 }
Write-Host "CONFLICT: $($conflicts.Count) file(s); resolve on the branch (merge is left open)"
foreach ($c in $conflicts) { Write-Host ("  {0}{1}" -f $(if (@($HotFiles | Where-Object { $c -like $_ }).Count -gt 0) { '[hot] ' } else { '' }), $c) }
exit 2
